using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using Teeditor.Common.Models.Commands;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Factories;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data
{
    internal class MapEnvelope : MapItem
    {
        private string _name;
        private int _channelsNumber;
        private bool _isSynchronized;
        private ObservableCollection<MapEnvelopePoint> _points;
        private static MapItemFactory _pointFactory;

        [ModificationCommandLabel("Envelope name changed")]
        public string Name
        {
            get => string.IsNullOrEmpty(_name) ? "Envelope" : _name;
            set => Set(ref _name, value, nameof(_name));
        }

        public int ChannelsNumber
        {
            get => _channelsNumber;
            private set => Set(ref _channelsNumber, value);
        }

        [ModificationCommandLabel("Envelope synchronization changed")]
        public bool IsSynchronized
        {
            get => _isSynchronized;
            set => Set(ref _isSynchronized, value);
        }

        public EnvelopeType Type
        {
            get => ChannelsNumber == 3 ? EnvelopeType.Position : EnvelopeType.Color;
            private set
            {
                if (value == EnvelopeType.Position)
                    ChannelsNumber = 3;
                else
                    ChannelsNumber = 4;
            }
        }

        public MapEnvelopePoint FirstPoint => Points.FirstOrDefault();

        public MapEnvelopePoint LastPoint => Points.LastOrDefault();

        public ObservableCollection<MapEnvelopePoint> Points => _points;

        public MapItemFactory PointFactory => _pointFactory;

        public event EventHandler<EnvelopeCarrierChangedEventArgs> CarrierChanged;

        public MapEnvelope(EnvelopeType type)
        {
            _channelsNumber = type == EnvelopeType.Position ? 3 : 4;

            if (type == EnvelopeType.Position)
                _pointFactory = new MapEnvelopePointPositionFactory();
            else
                _pointFactory = new MapEnvelopePointColorFactory();

            _points = new ObservableCollection<MapEnvelopePoint>();
            _points.CollectionChanged += Points_CollectionChanged;
        }

        public void AddCarrier(MapLayer layer)
            => CarrierChanged?.Invoke(this, new EnvelopeCarrierChangedEventArgs(layer, null));

        public void RemoveCarrier(MapLayer layer)
            => CarrierChanged?.Invoke(this, new EnvelopeCarrierChangedEventArgs(null, layer));

        private float ApplyFunction(float a, int pointId, int channelId)
        {
            float v0 = CalcUtilities.ToFloat(Points[pointId].Values[channelId]);
            float v1 = CalcUtilities.ToFloat(Points[pointId + 1].Values[channelId]);
            return CalcUtilities.Mix(v0, v1, a);
        }

        private float ApplyLinearFunction(float a, int pointId, int channelId) => ApplyFunction(a, pointId, channelId);

        private float ApplyStepFunction(int pointId, int channelId)
        {
            var a = 0;
            return ApplyFunction(a, pointId, channelId);
        }

        private float ApplySmoothFunction(float a, int pointId, int channelId)
        {
            a = -2 * a * a * a + 3 * a * a; // Second hermite basis, h01(t)=-2*(t^2) + 3*(t^2)
            return ApplyFunction(a, pointId, channelId);
        }

        private float ApplySlowFunction(float a, int pointId, int channelId)
        {
            a = a * a * a;
            return ApplyFunction(a, pointId, channelId);
        }

        private float ApplyFastFunction(float a, int pointId, int channelId)
        {
            a = 1 - a;
            a = 1 - a * a * a;
            return ApplyFunction(a, pointId, channelId);
        }

        private float ApplyBezierFunction(float time, int pointId, int channelId)
        {
            // Monotonic 2d cubic bezier curve
            Vector2 p0, p1, p2, p3;
            Vector2 inTang, outTang;

            p0 = new Vector2((float)Points[pointId].Time.TotalMilliseconds / 1000.0f, CalcUtilities.ToFloat(Points[pointId].Values[channelId]));
            p3 = new Vector2((float)Points[pointId + 1].Time.TotalMilliseconds / 1000.0f, CalcUtilities.ToFloat(Points[pointId + 1].Values[channelId]));

            outTang = new Vector2(Points[pointId].OutTangentdx[channelId] / 1000.0f, CalcUtilities.ToFloat(Points[pointId].OutTangentdy[channelId]));
            inTang = new Vector2(-Points[pointId + 1].InTangentdx[channelId] / 1000.0f, -CalcUtilities.ToFloat(Points[pointId + 1].InTangentdy[channelId]));
            p1 = p0 + outTang;
            p2 = p3 - inTang;

            // Validate bezier curve
            CalcUtilities.ValidateFCurve(p0, ref p1, ref p2, p3);

            // Solve x(a) = time for a
            var a = Math.Clamp(CalcUtilities.SolveBezier(time / 1000.0f, p0.X, p1.X, p2.X, p3.X), 0.0f, 1.0f);

            // Value = y(t)
            return CalcUtilities.Bezier(p0.Y, p1.Y, p2.Y, p3.Y, a);
        }

        public bool TryEvaluateColor(float time, out float red, out float green, out float blue, out float alpha)
        {
            red = green = blue = alpha = 1;

            if (Type != EnvelopeType.Color)
                return false;

            Evaluate(time, 0, ref red);
            Evaluate(time, 1, ref green);
            Evaluate(time, 2, ref blue);
            Evaluate(time, 3, ref alpha);

            return true;
        }

        public bool TryEvaluatePosition(float time, out Vector2 offset, out float rotate)
        {
            offset = Vector2.Zero;
            rotate = 0;

            if (Type != EnvelopeType.Position)
                return false;

            Evaluate(time, 0, ref offset.X);
            Evaluate(time, 1, ref offset.Y);
            Evaluate(time, 2, ref rotate);
            rotate = rotate / 360.0f * MathF.PI * 2;

            return true;
        }

        private void Evaluate(float time, int channelId, ref float value)
        {
            if (Points.Count == 0)
            {
                value = 0;
                return;
            }

            if (Points.Count == 1)
            {
                value = CalcUtilities.ToFloat(Points[0].Values[channelId]);
                return;
            }

            time = time % (float)Points[Points.Count - 1].Time.TotalSeconds * 1000.0f;

            for (int i = 0; i < Points.Count - 1; i++)
            {
                if (time >= Points[i].Time.TotalMilliseconds && time <= Points[i + 1].Time.TotalMilliseconds)
                {
                    float delta = (float)Points[i + 1].Time.TotalMilliseconds - (float)Points[i].Time.TotalMilliseconds;
                    float a = (time - (float)Points[i].Time.TotalMilliseconds) / delta;

                    switch (Points[i].CurveType)
                    {
                        case CurveType.Linear:
                            value = ApplyLinearFunction(a, i, channelId);
                            return;

                        case CurveType.Step:
                            value = ApplyStepFunction(i, channelId);
                            return;

                        case CurveType.Smooth:
                            value = ApplySmoothFunction(a, i, channelId);
                            return;

                        case CurveType.Slow:
                            value = ApplySlowFunction(a, i, channelId);
                            return;

                        case CurveType.Fast:
                            value = ApplyFastFunction(a, i, channelId);
                            return;

                        case CurveType.Bezier:
                            value = ApplyBezierFunction(time, i, channelId);
                            return;
                    }
                }
            }

            value = CalcUtilities.ToFloat(Points[Points.Count - 1].Values[channelId]);
        }

        public event EventHandler PointsCollectionChanged;

        public event EventHandler<EnvelopePointTimeChangedEventArgs> PointTimeChanged;

        private void Points_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MapEnvelopePoint newPoint in e.NewItems)
                {
                    newPoint.PropertyChanged += Point_PropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (MapEnvelopePoint oldPoint in e.OldItems)
                {
                    oldPoint.PropertyChanged -= Point_PropertyChanged;
                }
            }

            PointsCollectionChanged?.Invoke(this, EventArgs.Empty);

            OnPropertyChanged("FirstPoint");
            OnPropertyChanged("LastPoint");
        }

        private void Point_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Time")
            {
                PointTimeChanged?.Invoke(this, new EnvelopePointTimeChangedEventArgs((MapEnvelopePoint)sender));
            }
        }
    }

    internal struct EnvelopePointTimeChangedEventArgs
    {
        public MapEnvelopePoint Point { get; }

        public EnvelopePointTimeChangedEventArgs(MapEnvelopePoint point)
        {
            Point = point;
        }
    }
}
