using System;
using System.Threading.Tasks;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload.Strategies
{
    internal class MapFilePayloadQuadsLayerAddingStrategy : MapFilePayloadPartAddingStrategy
    {
        public override async Task<IMapItemDTO> AddAsync(MapItem mapItem)
        {
            var mapLayer = (MapQuadsLayer)mapItem;
            var dto = new MapQuadsLayerDTO();

            dto.version = (int)ItemCurrentVersion.QuadsLayer;
            dto.baseType = (int)LayerType.Quads;

            var isHighDetailFlag = mapLayer.IsHighDetail ? LayerBaseFlags.IsHighDetail : LayerBaseFlags.None;
            dto.baseFlags = (int)isHighDetailFlag;

            dto.baseVersion = 0; // Unused

            dto.quadsNumber = mapLayer.Quads.Count;
            dto.imageId = mapLayer.ImageId;

            dto.quadsDataIndex = -1;

            var quadsData = QuadsPacker.Pack(mapLayer.Quads);

            if (quadsData.Length > 0)
                dto.quadsDataIndex = await _payload.Data.TryAddDecompressedAsync(quadsData);
            
            dto.nameArray = mapLayer.Name.StrToInts(3);

            _payload.Items.LayerDTOs.Add(dto);

            return dto;
        }

        public override void Add(IMapItemDTO mapItemDTO)
            => _payload.Items.LayerDTOs.Add(mapItemDTO);

        protected override Type GetItemDtoType(int version)
        {
            switch (version)
            {
                case 1:
                    return typeof(MapQuadsLayerDTO_v1);
                default:
                    return typeof(MapQuadsLayerDTO);
            }
        }
    }
}
