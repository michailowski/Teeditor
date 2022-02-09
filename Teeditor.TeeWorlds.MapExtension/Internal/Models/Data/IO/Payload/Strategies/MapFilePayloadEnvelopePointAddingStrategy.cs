using System;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload.Strategies
{
    internal class MapFilePayloadEnvelopePointAddingStrategy : MapFilePayloadPartAddingStrategy
    {
        public override IMapItemDTO Add(MapItem mapItem)
        {
            var mapEnvelopePoint = (MapEnvelopePoint)mapItem;
            var dto = new MapEnvelopePointDTO();

            dto.time = (int)mapEnvelopePoint.Time.TotalMilliseconds;
            dto.curveType = (int)mapEnvelopePoint.CurveType;
            dto.values = mapEnvelopePoint.Values;
            dto.inTangentdx = mapEnvelopePoint.InTangentdx;
            dto.inTangentdy = mapEnvelopePoint.InTangentdy;
            dto.outTangentdx = mapEnvelopePoint.OutTangentdx;
            dto.outTangentdy = mapEnvelopePoint.OutTangentdy;

            _payload.Items.EnvelopePointDTOs.Add(dto);

            return dto;
        }

        public override void Add(IMapItemDTO mapItemDTO)
            => _payload.Items.EnvelopePointDTOs.Add(mapItemDTO);

        protected override Type GetItemDtoType(int version)
        {
            switch (version)
            {
                case 1:
                    return typeof(MapEnvelopePointDTO_v1);
                default:
                    return typeof(MapEnvelopePointDTO);
            }
        }
    }
}
