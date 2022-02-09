using System;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload.Strategies
{
    internal class MapFilePayloadGroupAddingStrategy : MapFilePayloadPartAddingStrategy
    {
        public override IMapItemDTO Add(MapItem mapItem)
        {
            var mapGroup = (MapGroup)mapItem;
            var dto = new MapGroupDTO();

            dto.version = (int)ItemCurrentVersion.Group;
            dto.nameArray = mapGroup.Name.StrToInts(3);
            dto.offsetX = (int)mapGroup.Offset.X;
            dto.offsetY = (int)mapGroup.Offset.Y;
            dto.parallaxX = (int)mapGroup.Parallax.X;
            dto.parallaxY = (int)mapGroup.Parallax.Y;
            dto.useClipping = Convert.ToInt32(mapGroup.UseClipping);
            dto.clipX = (int)mapGroup.Clip.X;
            dto.clipY = (int)mapGroup.Clip.Y;
            dto.clipW = (int)mapGroup.Clip.Width;
            dto.clipH = (int)mapGroup.Clip.Height;
            dto.layersNumber = mapGroup.Layers.Count;
            dto.startLayerIndex = _payload.Items.LayerDTOs.Count;

            _payload.Items.GroupDTOs.Add(dto);

            return dto;
        }

        public override void Add(IMapItemDTO mapItemDTO)
            => _payload.Items.GroupDTOs.Add(mapItemDTO);

        protected override Type GetItemDtoType(int version)
        {
            switch (version)
            {
                case 1:
                    return typeof(MapGroupDTO_v1);
                case 2:
                    return typeof(MapGroupDTO_v2);
                default:
                    return typeof(MapGroupDTO);
            }
        }
    }
}
