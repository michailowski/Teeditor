using System;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Factories
{
    internal class MapImageFactory : MapItemFactory
    {
        public override MapItem Create()
        {
            var mapImage = new MapImage();

            return mapImage;
        }

        public override MapItem Create(IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            MapImage mapImage = null;

            TryCreate_v1(ref mapImage, mapItemDTO, payload);

            var mapImageDTO = mapItemDTO as MapImageDTO;
            
            if (mapImageDTO != null)
            {
                mapImage.Format = mapImageDTO.format;
            }

            return mapImage;
        }

        private void TryCreate_v1(ref MapImage mapImage, IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            var mapImageDTO = mapItemDTO as MapImageDTO_v1;

            if (mapImageDTO == null)
                return;

            mapImage = new MapImage();

            mapImage.Width = mapImageDTO.width;
            mapImage.Height = mapImageDTO.height;
            mapImage.IsExternal = Convert.ToBoolean(mapImageDTO.isExternal);

            payload.Data.TryGetDecompressed(mapImageDTO.nameDataIndex, out string name);
            mapImage.Name = name;

            payload.Data.TryGetDecompressed(mapImageDTO.imageDataIndex, out byte[] imageData);
            mapImage.Data = imageData;

            TryCreate_v2(ref mapImage, mapItemDTO, payload);
        }

        private void TryCreate_v2(ref MapImage mapImage, IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            var mapImageDTO = mapItemDTO as MapImageDTO;

            if (mapImageDTO == null)
                return;

            mapImage.Format = mapImageDTO.format;
        }
    }
}
