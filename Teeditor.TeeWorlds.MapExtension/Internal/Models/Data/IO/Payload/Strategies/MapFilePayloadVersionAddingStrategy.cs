using System;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload.Strategies
{
    internal class MapFilePayloadVersionAddingStrategy : MapFilePayloadPartAddingStrategy
    {
        public override IMapItemDTO Add(MapItem mapItem)
        {
            var dto = new MapVersionDTO();

            dto.version = (int)ItemCurrentVersion.Map;

            _payload.Items.VersionDTO = dto;

            return dto;
        }

        public override void Add(IMapItemDTO mapItemDTO)
            => _payload.Items.VersionDTO = mapItemDTO;

        protected override Type GetItemDtoType(int version) => typeof(MapVersionDTO);
    }
}
