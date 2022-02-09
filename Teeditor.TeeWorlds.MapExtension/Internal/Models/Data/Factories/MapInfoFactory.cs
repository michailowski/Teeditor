using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Factories
{
    internal class MapInfoFactory : MapItemFactory
    {
        public override MapItem Create()
        {
            var mapInfo = new MapInfo();

            return mapInfo;
        }

        public override MapItem Create(IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            var mapInfoDTO = mapItemDTO as MapInfoDTO;
            var mapInfo = new MapInfo();

            if (mapInfoDTO == null)
                return mapInfo;

            payload.Data.TryGetDecompressed(mapInfoDTO.authorDataIndex, out string author);
            mapInfo.Author = author;

            payload.Data.TryGetDecompressed(mapInfoDTO.mapVersionDataIndex, out string mapVersion);
            mapInfo.MapVersion = mapVersion;

            payload.Data.TryGetDecompressed(mapInfoDTO.licenseDataIndex, out string license);
            mapInfo.License = license;

            payload.Data.TryGetDecompressed(mapInfoDTO.creditsDataIndex, out string credits);
            mapInfo.Credits = credits;

            return mapInfo;
        }
    }
}
