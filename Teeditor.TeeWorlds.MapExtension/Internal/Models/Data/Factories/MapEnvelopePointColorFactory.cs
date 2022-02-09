using System;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Factories
{
    internal class MapEnvelopePointColorFactory : MapItemFactory
    {
        private TimeSpan _time = TimeSpan.Zero;

        public override MapItem Create()
        {
            var mapEnvelopePoint = new MapEnvelopePointColor(_time);

            return mapEnvelopePoint;
        }

        public void SetTime(TimeSpan time) => _time = time;

        public override MapItem Create(IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            MapEnvelopePointColor mapEnvelopePoint = null;

            TryCreate_v2(ref mapEnvelopePoint, mapItemDTO, payload);

            return mapEnvelopePoint;
        }

        private void TryCreate_v1(out MapEnvelopePointColor mapEnvelopePoint, IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            mapEnvelopePoint = null;

            var mapEnvelopePointDTO = mapItemDTO as MapEnvelopePointDTO_v1;

            if (mapEnvelopePointDTO == null)
                return;

            var time = TimeSpan.FromMilliseconds(mapEnvelopePointDTO.time);

            mapEnvelopePoint = new MapEnvelopePointColor(time, mapEnvelopePointDTO.values);
            mapEnvelopePoint.CurveType = (CurveType)mapEnvelopePointDTO.curveType;
        }

        private void TryCreate_v2(ref MapEnvelopePointColor mapEnvelopePoint, IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            var mapEnvelopePointDTO = mapItemDTO as MapEnvelopePointDTO;

            if (mapEnvelopePointDTO == null)
            {
                TryCreate_v1(out mapEnvelopePoint, mapItemDTO, payload);
                return;
            }

            var time = TimeSpan.FromMilliseconds(mapEnvelopePointDTO.time);

            mapEnvelopePoint = new MapEnvelopePointColor(
                time,
                mapEnvelopePointDTO.values,
                mapEnvelopePointDTO.inTangentdx,
                mapEnvelopePointDTO.inTangentdy,
                mapEnvelopePointDTO.outTangentdx,
                mapEnvelopePointDTO.outTangentdy);

            mapEnvelopePoint.CurveType = (CurveType)mapEnvelopePointDTO.curveType;
        }
    }
}
