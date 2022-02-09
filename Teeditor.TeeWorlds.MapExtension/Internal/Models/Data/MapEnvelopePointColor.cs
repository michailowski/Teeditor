using System;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;
using Windows.UI;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data
{
    internal class MapEnvelopePointColor : MapEnvelopePoint
    {
        public byte Red
        {
            get => (byte)(CalcUtilities.ToFloat(_values[0]) * 255f);
            set
            {
                _values[0] = CalcUtilities.ToFixed(value / 255f);
                OnPropertyChanged();
            }
        }

        public byte Green
        {
            get => (byte)(CalcUtilities.ToFloat(_values[1]) * 255f);
            set
            {
                _values[1] = CalcUtilities.ToFixed(value / 255f);
                OnPropertyChanged();
            }
        }

        public byte Blue
        {
            get => (byte)(CalcUtilities.ToFloat(_values[2]) * 255f);
            set
            {
                _values[2] = CalcUtilities.ToFixed(value / 255f);
                OnPropertyChanged();
            }
        }

        public byte Alpha
        {
            get => (byte)(CalcUtilities.ToFloat(_values[3]) * 255f);
            set
            {
                _values[3] = CalcUtilities.ToFixed(value / 255f);
                OnPropertyChanged();
            }
        }

        public Color Color
        {
            get => Color.FromArgb(Alpha, Red, Green, Blue);
            set
            {
                Alpha = value.A;
                Red = value.R;
                Green = value.G;
                Blue = value.B;
                OnPropertyChanged();
            }
        }

        public MapEnvelopePointColor(TimeSpan time) : base()
        {
            _time = time;
            Red = Green = Blue = Alpha = 255;
        }

        public MapEnvelopePointColor(TimeSpan time, int[] values) : base()
        {
            _time = time;
            _values = values;
        }

        public MapEnvelopePointColor(TimeSpan time, int[] values, int[] inTangentdx, int[] inTangentdy, int[] outTangentdx, int[] outTangentdy)
            : base(time, values, inTangentdx, inTangentdy, outTangentdx, outTangentdy)
        {
        }
    }
}
