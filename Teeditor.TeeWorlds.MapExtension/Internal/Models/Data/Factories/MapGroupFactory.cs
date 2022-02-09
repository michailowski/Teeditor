using System;
using System.Numerics;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;
using Windows.Foundation;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Factories
{
    internal class MapGroupFactory : MapItemFactory
    {
        public override MapItem Create()
        {
            var mapGroup = new MapGroup();

            return mapGroup;
        }

        public override MapItem Create(IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            var mapGroup = new MapGroup();

            TryCreate_v1(ref mapGroup, mapItemDTO, payload);

            return mapGroup;
        }

        private void TryCreate_v1(ref MapGroup mapGroup, IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            var mapGroupDTO = mapItemDTO as MapGroupDTO_v1;

            if (mapGroupDTO == null)
                return;

            mapGroup.Offset = new Vector2(mapGroupDTO.offsetX, mapGroupDTO.offsetY);
            mapGroup.Parallax = new Vector2(mapGroupDTO.parallaxX, mapGroupDTO.parallaxY);

            TryCreate_v2(ref mapGroup, mapItemDTO, payload);
        }

        private void TryCreate_v2(ref MapGroup mapGroup, IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            var mapGroupDTO = mapItemDTO as MapGroupDTO_v2;

            if (mapGroupDTO == null)
                return;

            mapGroup.UseClipping = Convert.ToBoolean(mapGroupDTO.useClipping);

            mapGroup.Clip = new Rect()
            {
                X = mapGroupDTO.clipX,
                Y = mapGroupDTO.clipY,
                Width = mapGroupDTO.clipW,
                Height = mapGroupDTO.clipH
            };

            TryCreate_v3(ref mapGroup, mapItemDTO, payload);
        }

        private void TryCreate_v3(ref MapGroup mapGroup, IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            var mapGroupDTO = mapItemDTO as MapGroupDTO;

            if (mapGroupDTO == null)
                return;

            mapGroup.Name = mapGroupDTO.nameArray.IntsToStr();
        }
    }
}
