using System;
using System.Threading.Tasks;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload.Strategies
{
    internal class MapFilePayloadTilesLayerAddingStrategy : MapFilePayloadPartAddingStrategy
    {
        public override async Task<IMapItemDTO> AddAsync(MapItem mapItem)
        {
            var mapLayer = mapItem as MapTilesLayer;

            if (mapLayer == null)
                return null;

            var dto = new MapTilesLayerDTO();

            dto.version = (int)ItemCurrentVersion.TilesLayer;
            dto.baseType = (int)LayerType.Tiles;

            var isHighDetailFlag = mapLayer.IsHighDetail ? LayerBaseFlags.IsHighDetail : LayerBaseFlags.None;
            dto.baseFlags = (int)isHighDetailFlag;

            dto.baseVersion = 0; // Unused

            dto.width = mapLayer.Width;
            dto.height = mapLayer.Height;

            var isGameLayerFlag = mapLayer.IsGameLayer ? TilesLayerFlags.IsGameLayer : TilesLayerFlags.None;
            dto.flags = (int)isGameLayerFlag;

            dto.color = mapLayer.ColorChannels;
            dto.colorEnvelopeId = mapLayer.ColorEnvelopeId;
            dto.colorEnvelopeOffset = mapLayer.ColorEnvelopeOffset;
            dto.imageId = mapLayer.ImageId;

            dto.tilesDataIndex = -1;

            var tilesData = TilesPacker.Pack(mapLayer.Tiles, mapLayer.Width, mapLayer.Height);
            
            if (tilesData.Length > 0)
                dto.tilesDataIndex = await _payload.Data.TryAddDecompressedAsync(tilesData);

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
                    return typeof(MapTilesLayerDTO_v1);
                case 2:
                    return typeof(MapTilesLayerDTO_v2);
                case 3:
                    return typeof(MapTilesLayerDTO_v3);
                default:
                    return typeof(MapTilesLayerDTO);
            }
        }
    }
}
