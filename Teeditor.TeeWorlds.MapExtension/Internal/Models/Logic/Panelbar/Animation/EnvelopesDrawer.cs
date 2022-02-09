using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Toolkit.Uwp.UI.Media.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.Panelbar.Animation
{
    internal class EnvelopesDrawer
    {
        private Color _envelopeBackgroundColor;
        private Color _envelopeBorderColor;
        private Color _envelopeLineColor;
        private Color _envelopeLinePointerOverColor;
        private Color _keyframeColor;
        private Color _keyframePointerOverColor;
        private Color _keyframeStackedCounterColor;

        private double _width;
        private double _height;
        private double _marginRight = 50;
        private double _marginLeft;

        private double _envelopeHeight;
        private int _secondLengthInPixels;

        private Point _pointerPosition;

        private CanvasGeometry _keyframeGeometry;
        private Rect _keyframeGeometryBounds;

        private EnvelopesContainer _envelopesContainer;

        private int _hoveredEnvelopeIndex;
        private MapEnvelope _hoveredEnvelope;

        private int _invalidatedEnvelopeIndex = -1;

        private KeyframeTranslator _keyframeTranslator;

        private CanvasTextFormat _stackedCounterFormat;

        public Keyframe HoveredKeyframe { get; private set; }
        public EnvelopeSegment HoveredSegment { get; private set; }


        public event EventHandler ContentBoundsChanged;

        public event EventHandler ForceRegionsInvalidation;

        public EnvelopesDrawer(int secondLengthInPixels)
        {
            _secondLengthInPixels = secondLengthInPixels;
            _keyframeTranslator = new KeyframeTranslator(secondLengthInPixels);

            InitStyles();
        }

        private void InitStyles()
        {
            var resources = Application.Current.Resources;

            _marginLeft = (double)resources["EnvelopeContainerPaddingLeft"];
            _envelopeHeight = (double)resources["EnvelopeContainerHeight"];
            _envelopeBackgroundColor = (resources["EnvelopeContainerBackground"] as SolidColorBrush)?.Color ?? Colors.Transparent;
            _envelopeBorderColor = (resources["EnvelopeContainerBorderBrush"] as SolidColorBrush)?.Color ?? Colors.Transparent;
            _envelopeLineColor = (resources["EnvelopeLineBackground"] as SolidColorBrush)?.Color ?? Colors.Transparent;
            _envelopeLinePointerOverColor = (resources["EnvelopeLineBackgroundPointerOver"] as SolidColorBrush)?.Color ?? Colors.Transparent;
            _keyframeColor = (resources["KeyframeFill"] as SolidColorBrush)?.Color ?? Colors.Transparent;
            _keyframePointerOverColor = (resources["KeyframeFillPointerOver"] as SolidColorBrush)?.Color ?? Colors.Transparent;
            _keyframeStackedCounterColor = (resources["KeyframeStackedCounterForeground"] as SolidColorBrush)?.Color ?? Colors.Transparent;

            var keyframePath = (string)resources["KeyframeIconPath"];
            _keyframeGeometry = CanvasPathGeometry.CreateGeometry(CanvasDevice.GetSharedDevice(), keyframePath);
            _keyframeGeometryBounds = _keyframeGeometry.ComputeBounds();

            _stackedCounterFormat = new CanvasTextFormat();
            _stackedCounterFormat.FontSize = 9;
            _stackedCounterFormat.FontWeight = new FontWeight() { Weight = 800 };
        }

        #region Get and Set Methods

        public void SetEnvelopes(EnvelopesContainer envelopesContainer)
        {
            RemoveEnvelopesHandlers();

            _envelopesContainer = envelopesContainer;

            AddEnvelopesHandlers();

            CalculateBounds();
        }

        public void SetSecondLength(int secondLengthInPixels)
        {
            _secondLengthInPixels = secondLengthInPixels;
            _keyframeTranslator.SetSecondLength(secondLengthInPixels);
        }

        public void SetPointerPosition(Point pointerPosition)
        {
            _pointerPosition = pointerPosition;

            if (_keyframeTranslator.IsActive)
                return;

            DetectHoveredEnvelope();

            if (IsElementsMouseStateChanged())
            {
                ForceRegionsInvalidation?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ResetPointerPosition()
            => _pointerPosition = new Point(-1, -1);

        private double GetPixelsByTime(TimeSpan time) => time.TotalSeconds * _secondLengthInPixels;

        public Rect GetInvalidatedRegion(Rect canBeInvalidate)
        {
            if (_keyframeTranslator.IsActive && _keyframeTranslator.IsBoosterActive == false)
            {
                if (_keyframeTranslator.IsMovedByOffset)
                    return Rect.Empty;

                return GetEnvelopeInvalidatedRect(_hoveredEnvelopeIndex, canBeInvalidate);
            }

            if (_invalidatedEnvelopeIndex >= 0 && _keyframeTranslator.IsBoosterActive == false)
            {
                return GetEnvelopeInvalidatedRect(_invalidatedEnvelopeIndex, canBeInvalidate);
            }

            var isMouseStateChanged = IsElementsMouseStateChanged();

            if (isMouseStateChanged != false)
            {
                return GetEnvelopeInvalidatedRect(_hoveredEnvelopeIndex, canBeInvalidate);
            }

            return Rect.Empty;
        }

        private Rect GetEnvelopeInvalidatedRect(int envelopeIndex, Rect canBeInvalidate)
            => new Rect(canBeInvalidate.Left, envelopeIndex * _envelopeHeight, canBeInvalidate.Width, _envelopeHeight);

        public Rect GetContentBounds()
            => new Rect(0, 0, _width + _marginRight, _height);

        #endregion

        #region Envelopes Handlers

        private void AddEnvelopesHandlers()
        {
            if (_envelopesContainer == null)
                return;

            (_envelopesContainer.Items as INotifyCollectionChanged).CollectionChanged += Envelopes_CollectionChanged;

            foreach (var envelope in _envelopesContainer.Items)
            {
                envelope.PointTimeChanged += Envelope_PointTimeChanged;
                envelope.PointsCollectionChanged += Envelope_PointsCollectionChanged;
            }
        }

        private void RemoveEnvelopesHandlers()
        {
            if (_envelopesContainer == null)
                return;

            (_envelopesContainer.Items as INotifyCollectionChanged).CollectionChanged -= Envelopes_CollectionChanged;

            foreach (var envelope in _envelopesContainer.Items)
            {
                envelope.PointTimeChanged -= Envelope_PointTimeChanged;
                envelope.PointsCollectionChanged -= Envelope_PointsCollectionChanged;
            }
        }

        private void Envelopes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MapEnvelope newEnvelope in e.NewItems)
                {
                    newEnvelope.PointTimeChanged += Envelope_PointTimeChanged;
                    newEnvelope.PointsCollectionChanged += Envelope_PointsCollectionChanged;
                }

                CalculateBounds();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (MapEnvelope oldEnvelope in e.OldItems)
                {
                    oldEnvelope.PointTimeChanged -= Envelope_PointTimeChanged;
                    oldEnvelope.PointsCollectionChanged -= Envelope_PointsCollectionChanged;
                }

                CalculateBounds();
            }

            ForceRegionsInvalidation?.Invoke(this, EventArgs.Empty);
        }

        private void Envelope_PointsCollectionChanged(object sender, EventArgs e)
        {
            var envelope = (MapEnvelope)sender;

            _invalidatedEnvelopeIndex = _envelopesContainer.Items.IndexOf(envelope);

            CalculateBounds();

            ForceRegionsInvalidation?.Invoke(this, EventArgs.Empty);

            _invalidatedEnvelopeIndex = -1;
        }

        private void Envelope_PointTimeChanged(object sender, EnvelopePointTimeChangedEventArgs e)
        {
            var envelope = (MapEnvelope)sender;

            _invalidatedEnvelopeIndex = _envelopesContainer.Items.IndexOf(envelope);

            if (envelope.LastPoint == e.Point)
            {
                var pixels = GetPixelsByTime(e.Point.Time);

                if (pixels > _width)
                {
                    _width = pixels;
                    ContentBoundsChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            ForceRegionsInvalidation?.Invoke(this, EventArgs.Empty);

            _invalidatedEnvelopeIndex = -1;
        }

        #endregion

        #region Draw Methods

        public void Draw(CanvasDrawingSession ds, Rect region)
        {
            int start = (int)Math.Floor(region.Top / _envelopeHeight);
            int end = (int)Math.Ceiling(region.Bottom / _envelopeHeight);

            start = Math.Max(start, 0);
            end = Math.Min(end, _envelopesContainer.Items.Count);

            for (int i = start; i < end; i++)
            {
                ds.Antialiasing = CanvasAntialiasing.Aliased;

                DrawEnvelopeContainer(ds, region, i);

                DrawEnvelopeLine(ds, region, _envelopesContainer.Items[i], i);

                DrawSegment(ds, region);

                ds.Antialiasing = CanvasAntialiasing.Antialiased;

                DrawEnvelopeKeyframes(ds, region, _envelopesContainer.Items[i], i);
            }
        }

        private void DrawEnvelopeContainer(CanvasDrawingSession ds, Rect region, int envelopeIndex)
        {
            var envelopeContainerRect = new Rect()
            {
                X = (float)region.Left,
                Y = envelopeIndex * _envelopeHeight,
                Width = (float)region.Width,
                Height = _envelopeHeight
            };

            ds.FillRectangle(envelopeContainerRect, _envelopeBackgroundColor);

            var bottomBorderY = (float)(envelopeIndex * _envelopeHeight + _envelopeHeight);

            ds.DrawLine((float)region.Left, bottomBorderY, (float)region.Right, bottomBorderY, _envelopeBorderColor);
        }

        private void DrawEnvelopeKeyframes(CanvasDrawingSession ds, Rect region, MapEnvelope envelope, int envelopeIndex)
        {
            Vector2 lastKeyframePosition = new Vector2(-1);
            int stackedKeyframesCount = 1;

            foreach (var point in envelope.Points)
            {
                var x = GetPixelsByTime(point.Time) + _marginLeft;

                if (x >= region.Left && x <= region.Right)
                {
                    var y = envelopeIndex * _envelopeHeight + _envelopeHeight / 2;

                    var keyframePosition = new Vector2()
                    {
                        X = (float)(x - _keyframeGeometryBounds.Width / 2),
                        Y = (float)(y - _keyframeGeometryBounds.Height / 2)
                    };

                    var color = point == HoveredKeyframe.Point ? _keyframePointerOverColor : _keyframeColor;

                    ds.FillGeometry(_keyframeGeometry, keyframePosition, color);

                    if (lastKeyframePosition == keyframePosition)
                    {
                        stackedKeyframesCount++;
                        ds.DrawText(stackedKeyframesCount.ToString(), keyframePosition.X + 3, keyframePosition.Y, _keyframeStackedCounterColor, _stackedCounterFormat);
                    }
                    else
                    {
                        stackedKeyframesCount = 1;
                    }

                    lastKeyframePosition = keyframePosition;
                }
            }
        }

        private void DrawEnvelopeLine(CanvasDrawingSession ds, Rect region, MapEnvelope envelope, int envelopeIndex)
        {
            if (envelope.FirstPoint == null || envelope.LastPoint == null)
                return;

            var startX = GetPixelsByTime(envelope.FirstPoint.Time) + _marginLeft;
            var endX = GetPixelsByTime(envelope.LastPoint.Time) + _marginLeft;

            if (endX < region.Left || startX > region.Right)
                return;

            var y = envelopeIndex * _envelopeHeight + _envelopeHeight / 2;
            var height = 6;

            var rect = new Rect()
            {
                X = startX,
                Y = y - height / 2,
                Width = endX - startX,
                Height = height
            };

            ds.FillRoundedRectangle(rect, 3, 3, _envelopeLineColor);
        }

        private void DrawSegment(CanvasDrawingSession ds, Rect region)
        {
            if (HoveredSegment.IsEmpty == true)
                return;

            ds.FillRoundedRectangle(HoveredSegment.Bounds, 2, 2, _envelopeLinePointerOverColor);
        }

        #endregion

        #region Elements Mouse State Methods

        private bool IsElementsMouseStateChanged()
        {
            if (_hoveredEnvelope == null)
                return false;

            var isKeyframeMouseLeave = IsKeyframeMouseLeaved();
            var isSegmentMouseLeave = IsSegmentMouseLeaved();

            var isKeyframeMouseOver = false;
            var isSegmentMouseOver = false;

            for (int i = _hoveredEnvelope.Points.Count - 1; i >= 0; i--)
            {
                if (isKeyframeMouseOver == false)
                {
                    isKeyframeMouseOver = IsKeyframeMouseOvered(_hoveredEnvelope.Points[i]);
                }

                int prevKeyframeIndex = Math.Max(0, i - 1);

                if (prevKeyframeIndex < i)
                {
                    isSegmentMouseOver = IsSegmentMouseOvered(_hoveredEnvelope.Points[prevKeyframeIndex], _hoveredEnvelope.Points[i]);

                    if (isSegmentMouseOver)
                        break;
                }
            }

            return isKeyframeMouseOver || isKeyframeMouseLeave || isSegmentMouseOver || isSegmentMouseLeave;
        }

        private bool IsKeyframeMouseLeaved()
        {
            if (HoveredKeyframe.IsEmpty == true || HoveredKeyframe.Bounds.Contains(_pointerPosition) == true)
                return false;

            HoveredKeyframe = Keyframe.Empty;

            return true;
        }

        private bool IsKeyframeMouseOvered(MapEnvelopePoint envelopePoint)
        {
            if (HoveredKeyframe.IsEmpty == false)
                return false;

            var x = GetPixelsByTime(envelopePoint.Time) + _marginLeft;
            var y = _hoveredEnvelopeIndex * _envelopeHeight + _envelopeHeight / 2;

            var keyframeBoundsRect = new Rect()
            {
                X = x - _keyframeGeometryBounds.Width / 2,
                Y = y - _keyframeGeometryBounds.Height / 2,
                Width = _keyframeGeometryBounds.Width,
                Height = _keyframeGeometryBounds.Height
            };

            if (keyframeBoundsRect.Contains(_pointerPosition) == false)
                return false;

            HoveredKeyframe = new Keyframe(_hoveredEnvelope, envelopePoint, keyframeBoundsRect);

            return true;
        }

        private bool IsSegmentMouseLeaved()
        {
            if (HoveredSegment.IsEmpty == true || HoveredSegment.Bounds.Contains(_pointerPosition) == true)
                return false;

            HoveredSegment = EnvelopeSegment.Empty;

            return true;
        }

        private bool IsSegmentMouseOvered(MapEnvelopePoint envelopePointLeft, MapEnvelopePoint envelopePointRight)
        {
            if (HoveredSegment.IsEmpty == false)
                return false;

            var leftKeyframeX = GetPixelsByTime(envelopePointLeft.Time) + _marginLeft + (float)_keyframeGeometryBounds.Width / 2 + 4;
            var rightKeyframeX = GetPixelsByTime(envelopePointRight.Time) + _marginLeft - (float)_keyframeGeometryBounds.Width / 2 - 4;

            var segmentBoundsRect = new Rect()
            {
                X = leftKeyframeX,
                Y = _hoveredEnvelopeIndex * _envelopeHeight + _envelopeHeight / 2 - 6 / 2,
                Width = Math.Max(0, rightKeyframeX - leftKeyframeX),
                Height = 6
            };

            if (segmentBoundsRect.Contains(_pointerPosition) == false)
                return false;

            HoveredSegment = new EnvelopeSegment(_hoveredEnvelope, envelopePointLeft, envelopePointRight, segmentBoundsRect);

            return true;
        }

        #endregion

        #region Dragging and Move Methods

        public bool TryStartDragging(Point pointerPosition)
            => _keyframeTranslator.TryActivate(HoveredKeyframe, pointerPosition);

        public bool TryDragging(Point pointerPosition)
            => _keyframeTranslator.TryTranslate(pointerPosition);

        public void EndDragging()
        {
            if (_keyframeTranslator.IsActive && _keyframeTranslator.Keyframe.Point == _keyframeTranslator.Keyframe.Envelope.LastPoint)
            {
                var pixels = GetPixelsByTime(_keyframeTranslator.Keyframe.Point.Time);

                CalculateBounds();
            }

            _keyframeTranslator.Deactivate();
        }

        public bool TryActivateDragBoosting(KeyframeBoostingDirection boostingDirection)
            => _keyframeTranslator.TryActivateBooster(boostingDirection);

        public void DeactivateDragBoosting()
            => _keyframeTranslator.DeactivateBooster();

        public bool TryMoveByOffset(int offsex)
        { 
            var isMoved = _keyframeTranslator.TryMoveByOffset(offsex);

            if (isMoved)
            {
                _invalidatedEnvelopeIndex = -1;
                ForceRegionsInvalidation?.Invoke(this, EventArgs.Empty);
            }

            return isMoved;
        }

        #endregion

        private void DetectHoveredEnvelope()
        {
            var envelopeIndex = (int)(_pointerPosition.Y / _envelopeHeight);

            if (envelopeIndex >= _envelopesContainer.Items.Count)
            {
                _hoveredEnvelopeIndex = -1;
                _hoveredEnvelope = null;
                return;
            }

            if (envelopeIndex == _hoveredEnvelopeIndex)
                return;

            _hoveredEnvelopeIndex = envelopeIndex;
            _hoveredEnvelope = _envelopesContainer.Items[_hoveredEnvelopeIndex];
        }

        private void CalculateBounds()
        {
            double width = 0;
            double height = 0;

            foreach (var envelope in _envelopesContainer.Items)
            {
                height += _envelopeHeight;

                if (envelope.Points.Count == 0)
                    continue;

                var keyframeX = GetPixelsByTime(envelope.Points[envelope.Points.Count - 1].Time);

                if (keyframeX > width)
                    width = keyframeX;
            }

            if (_width == width && _height == height)
                return;

            _width = width;
            _height = height;

            ContentBoundsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}