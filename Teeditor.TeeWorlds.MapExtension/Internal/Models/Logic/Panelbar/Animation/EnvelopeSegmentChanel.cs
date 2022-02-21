using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teeditor.Common.Models.Bindable;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.Panelbar.Animation
{
    internal class EnvelopeSegmentChanel : BindableBase
    {
        private readonly MapEnvelopePoint _leftEnvPoint;
        private readonly MapEnvelopePoint _rightEnvPoint;
        private readonly int _channelNumber;
        private readonly int _scale;
        private readonly Color _color;
        private bool _isVisible = true;

        public string Name { get; }
        public Point StartTangent
        {
            get => GetStartTangCoords();
            set
            {
                SetStartTangCoords(value);
                OnPropertyChanged();
            }
        }

        public Point EndTangent
        {
            get => GetEndTangCoords();
            set
            {
                SetEndTangCoords(value);
                OnPropertyChanged();
            }
        }

        public SolidColorBrush Brush => new SolidColorBrush(_color);

        public bool IsVisible
        {
            get => _isVisible;
            set => Set(ref _isVisible, value);
        }

        private float DeltaX
        {
            get
            {
                var a = (int)_rightEnvPoint.Time.TotalMilliseconds;
                var b = (int)_leftEnvPoint.Time.TotalMilliseconds;

                if (a >= b)
                    return a - b;
                else
                    return b - a;
            }
        }

        private float DeltaY
        {
            get
            {
                var a = CalcUtilities.ToFloat(_rightEnvPoint.Values[_channelNumber]);
                var b = CalcUtilities.ToFloat(_leftEnvPoint.Values[_channelNumber]);

                if (a >= b)
                    return a - b;
                else
                    return b - a;
            }
        }

        public EnvelopeSegmentChanel(int chanelNumber, MapEnvelopePoint leftEnvPoint, MapEnvelopePoint rightEnvPoint, int scale, string name, Color color)
        {
            _leftEnvPoint = leftEnvPoint;
            _rightEnvPoint = rightEnvPoint;
            _channelNumber = chanelNumber;
            _scale = scale;
            _color = color;
            Name = name;
        }

        private Point GetStartTangCoords()
        {
            var kX = _scale / DeltaX;
            var kY = DeltaY > 0 ? _scale / DeltaY : 0;

            var x = _leftEnvPoint.OutTangentdx[_channelNumber] * kX;
            var y = _scale - CalcUtilities.ToFloat(_leftEnvPoint.OutTangentdy[_channelNumber]) * kY;

            x = Math.Clamp(x, 0, _scale);
            y = Math.Clamp(y, 0, _scale);

            return new Point(x, y);
        }

        private void SetStartTangCoords(Point value)
        {
            var kX = _scale / DeltaX;
            var kY = _scale / DeltaY;

            var x = Math.Clamp(value.X, 0, _scale);
            var y = Math.Clamp(value.Y, 0, _scale);

            x = value.X / kX;
            y = (-value.Y + _scale) / kY;

            _leftEnvPoint.OutTangentdx[_channelNumber] = (int)x;
            _leftEnvPoint.OutTangentdy[_channelNumber] = CalcUtilities.ToFixed((float)y);
        }

        private Point GetEndTangCoords()
        {
            var kX = _scale / DeltaX;
            var kY = DeltaY > 0 ? _scale / DeltaY : 0;

            var x = _scale + _rightEnvPoint.InTangentdx[_channelNumber] * kX;
            var y = -CalcUtilities.ToFloat(_rightEnvPoint.InTangentdy[_channelNumber]) * kY;

            x = Math.Clamp(x, 0, _scale);
            y = Math.Clamp(y, 0, _scale);

            return new Point(x, y);
        }

        private void SetEndTangCoords(Point value)
        {
            var kX = _scale / DeltaX;
            var kY = _scale / DeltaY;

            var x = Math.Clamp(value.X, 0, _scale);
            var y = Math.Clamp(value.Y, 0, _scale);

            x = (value.X - _scale) / kX;
            y = -value.Y / kY;

            _rightEnvPoint.InTangentdx[_channelNumber] = (int)x;
            _rightEnvPoint.InTangentdy[_channelNumber] = CalcUtilities.ToFixed((float)y);
        }
    }
}
