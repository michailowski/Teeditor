using System;
using System.Threading.Tasks;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload.Strategies
{
    internal class MapFilePayloadInfoAddingStrategy : MapFilePayloadPartAddingStrategy
    {
        public override async Task<IMapItemDTO> AddAsync(MapItem mapItem)
        {
            var mapInfo = mapItem as MapInfo;

            var dto = new MapInfoDTO();

            _payload.Items.InfoDTO = dto;

            dto.version = (int)ItemCurrentVersion.Info;
            dto.authorDataIndex = dto.mapVersionDataIndex = dto.creditsDataIndex = dto.licenseDataIndex = -1;

            if (string.IsNullOrEmpty(mapInfo?.Author) == false)
                dto.authorDataIndex = await _payload.Data.TryAddDecompressedAsync(mapInfo.Author);

            if (string.IsNullOrEmpty(mapInfo?.MapVersion) == false)
                dto.mapVersionDataIndex = await _payload.Data.TryAddDecompressedAsync(mapInfo.MapVersion);

            if (string.IsNullOrEmpty(mapInfo?.Credits) == false)
                dto.creditsDataIndex = await _payload.Data.TryAddDecompressedAsync(mapInfo.Credits);

            if (string.IsNullOrEmpty(mapInfo?.License) == false)
                dto.licenseDataIndex = await _payload.Data.TryAddDecompressedAsync(mapInfo.License);

            return dto;
        }

        public override void Add(IMapItemDTO mapItemDTO)
            => _payload.Items.InfoDTO = mapItemDTO;

        protected override Type GetItemDtoType(int version) => typeof(MapInfoDTO);
    }
}
