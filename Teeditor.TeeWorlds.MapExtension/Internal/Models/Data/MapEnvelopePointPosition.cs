using System;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data
{
    internal class MapEnvelopePointPosition : MapEnvelopePoint
    {
        public float X
        {
            get => CalcUtilities.ToFloat(_values[0]);
            set
            {
                _values[0] = CalcUtilities.ToFixed(value);
                OnPropertyChanged();
            }
        }

        public float Y
        {
            get => CalcUtilities.ToFloat(_values[1]);
            set
            {
                _values[1] = CalcUtilities.ToFixed(value);
                OnPropertyChanged();
            }
        }

        public float Rotate
        {
            get => CalcUtilities.ToFloat(_values[2]);
            set
            {
                _values[2] = CalcUtilities.ToFixed(value);
                OnPropertyChanged();
            }
        }

        public MapEnvelopePointPosition(TimeSpan time) : base()
        {
            _time = time;
            X = Y = Rotate = 0;
        }

        public MapEnvelopePointPosition(TimeSpan time, int[] values) : base()
        {
            _time = time;
            _values = values;
        }

        public MapEnvelopePointPosition(TimeSpan time, int[] values, int[] inTangentdx, int[] inTangentdy, int[] outTangentdx, int[] outTangentdy)
            : base(time, values, inTangentdx, inTangentdy, outTangentdx, outTangentdy)
        {
        }
    }
}
