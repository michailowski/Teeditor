using System;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload.Strategies
{
    internal class MapFilePayloadEnvelopeAddingStrategy : MapFilePayloadPartAddingStrategy
    {
        public override IMapItemDTO Add(MapItem mapItem)
        {
            var mapEnvelope = (MapEnvelope)mapItem;
            var dto = new MapEnvelopeDTO();

            dto.version = (int)ItemCurrentVersion.Envelope;
            dto.nameArray = mapEnvelope.Name.StrToInts(8);
            dto.channelsNumber = mapEnvelope.ChannelsNumber;
            dto.isSynchronized = Convert.ToInt32(mapEnvelope.IsSynchronized);
            dto.pointsNumber = mapEnvelope.Points.Count;
            dto.startPointIndex = _payload.Items.EnvelopePointDTOs.Count;

            _payload.Items.EnvelopeDTOs.Add(dto);

            return dto;
        }

        public override void Add(IMapItemDTO mapItemDTO)
            => _payload.Items.EnvelopeDTOs.Add(mapItemDTO);

        protected override Type GetItemDtoType(int version)
        {
            switch (version)
            {
                case 1:
                    return typeof(MapEnvelopeDTO_v1);
                case 2:
                    return typeof(MapEnvelopeDTO_v2);
                default:
                    return typeof(MapEnvelopeDTO);
            }
        }
    }
}
