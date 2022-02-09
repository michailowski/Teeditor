using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Windows.Graphics.Imaging;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload.Strategies
{
    internal class MapFilePayloadImageAddingStrategy : MapFilePayloadPartAddingStrategy
    {
        public override async Task<IMapItemDTO> AddAsync(MapItem mapItem)
        {
            var mapImage = (MapImage)mapItem;
            var dto = new MapImageDTO();

            dto.version = (int)ItemCurrentVersion.Image;
            dto.width = mapImage.Width;
            dto.height = mapImage.Height;
            dto.isExternal = Convert.ToInt32(mapImage.IsExternal);

            dto.nameDataIndex = dto.imageDataIndex = -1;

            if (string.IsNullOrEmpty(mapImage.Name) == false)
                dto.nameDataIndex = await _payload.Data.TryAddDecompressedAsync(mapImage.Name);

            if (mapImage.Data != null && mapImage.IsExternal == false)
            {
                var softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(mapImage.Data.AsBuffer(), BitmapPixelFormat.Bgra8, mapImage.Width, mapImage.Height, BitmapAlphaMode.Premultiplied);
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Rgba8, BitmapAlphaMode.Straight);

                var buffer = new Windows.Storage.Streams.Buffer((uint)softwareBitmap.PixelWidth * (uint)softwareBitmap.PixelHeight * 4);
                softwareBitmap.CopyToBuffer(buffer);

                dto.imageDataIndex = await _payload.Data.TryAddDecompressedAsync(buffer.ToArray());
            }

            dto.format = (int)ImageFormat.RGBA;

            _payload.Items.ImageDTOs.Add(dto);

            return dto;
        }

        public override void Add(IMapItemDTO mapItemDTO)
            => _payload.Items.ImageDTOs.Add(mapItemDTO);

        protected override Type GetItemDtoType(int version)
        {
            switch (version)
            {
                case 1:
                    return typeof(MapImageDTO_v1);
                default:
                    return typeof(MapImageDTO);
            }
        }
    }
}
