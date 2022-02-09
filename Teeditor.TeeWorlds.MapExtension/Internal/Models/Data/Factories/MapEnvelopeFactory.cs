using System;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Factories
{
    internal class MapEnvelopeFactory : MapItemFactory
    {
        private EnvelopeType _type;

        public void SetType(EnvelopeType type)
            => _type = type;

        public override MapItem Create()
        {
            var mapEnvelope = new MapEnvelope(_type);

            return mapEnvelope;
        }

        public override MapItem Create(IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            TryCreate_v1(out var mapEnvelope, mapItemDTO, payload);

            return mapEnvelope;
        }

        private void TryCreate_v1(out MapEnvelope mapEnvelope, IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            mapEnvelope = null;

            var mapEnvelopeDTO = mapItemDTO as MapEnvelopeDTO_v1;

            if (mapEnvelopeDTO == null)
                return;

            var type = mapEnvelopeDTO.channelsNumber == 3 ? EnvelopeType.Position : EnvelopeType.Color;

            mapEnvelope = new MapEnvelope(type);
            mapEnvelope.Name = mapEnvelopeDTO.nameArray.IntsToStr();

            TryCreate_v2(ref mapEnvelope, mapItemDTO, payload);
        }

        private void TryCreate_v2(ref MapEnvelope mapEnvelope, IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            var mapEnvelopeDTO = mapItemDTO as MapEnvelopeDTO_v2;

            if (mapEnvelopeDTO == null)
                return;

            mapEnvelope.IsSynchronized = Convert.ToBoolean(mapEnvelopeDTO.isSynchronized);
        }
    }
}
