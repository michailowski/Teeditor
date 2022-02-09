using System;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data
{
    internal abstract class MapEnvelopePoint : MapItem
    {
        protected TimeSpan _time;
        private CurveType _curveType;
        protected int[] _values;
        protected int[] _inTangentdx;
        protected int[] _inTangentdy;
        protected int[] _outTangentdx;
        protected int[] _outTangentdy;

        public TimeSpan Time
        {
            get => _time;
            set => Set(ref _time, value);
        }

        public CurveType CurveType
        {
            get => _curveType;
            set
            {
                Set(ref _curveType, value);
                OnPropertyChanged("CurveTypeId");
            }
        }

        public int CurveTypeId
        {
            get => (int)_curveType;
            set => Set(ref _curveType, (CurveType)value);
        }

        public int[] Values => _values;

        public int[] InTangentdx => _inTangentdx;

        public int[] InTangentdy => _inTangentdy;

        public int[] OutTangentdx => _outTangentdx;

        public int[] OutTangentdy => _outTangentdy;

        public MapEnvelopePoint()
        {
            _values = new int[4];
            _inTangentdx = new int[4];
            _inTangentdy = new int[4];
            _outTangentdx = new int[4];
            _outTangentdy = new int[4];
        }

        public MapEnvelopePoint(TimeSpan time, int[] values, int[] inTangentdx, int[] inTangentdy, int[] outTangentdx, int[] outTangentdy)
        {
            _time = time;
            _values = values;
            _inTangentdx = inTangentdx;
            _inTangentdy = inTangentdy;
            _outTangentdx = outTangentdx;
            _outTangentdy = outTangentdy;
        }

        public void RaiseTimeChangeEvent()
        {
            OnPropertyChanged("Time");
        }
    }
}