using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;
using Windows.UI;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Factories
{
    internal class MapTilesLayerFactory : MapItemFactory
    {

        public override MapItem Create()
        {
            var mapLayer = new MapTilesLayer(50, 50);

            return mapLayer;
        }

        public MapTilesLayer CreateGameLayer()
        {
            var mapLayer = new MapTilesLayer(50, 50, true);

            return mapLayer;
        }

        public override MapItem Create(IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            MapTilesLayer mapLayer = null;

            TryCreate_v1(ref mapLayer, mapItemDTO, payload);

            return mapLayer;
        }

        private void TryCreate_v1(ref MapTilesLayer mapLayer, IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            mapLayer = null;
            var mapLayerDTO = mapItemDTO as MapTilesLayerDTO_v1;

            if (mapLayerDTO == null)
                return;

            var isGameLayer = (mapLayerDTO.flags & (byte)TilesLayerFlags.IsGameLayer) != 0;

            payload.Data.TryGetDecompressed(mapLayerDTO.tilesDataIndex, out byte[] tilesData);
            var tiles = TilesPacker.Unpack(tilesData, mapLayerDTO.version > 3);

            mapLayer = new MapTilesLayer(mapLayerDTO.width, mapLayerDTO.height, isGameLayer, tiles);

            mapLayer.Color = Color.FromArgb((byte)mapLayerDTO.color[3], (byte)mapLayerDTO.color[0], (byte)mapLayerDTO.color[1], (byte)mapLayerDTO.color[2]);
            mapLayer.ColorEnvelopeOffset = mapLayerDTO.colorEnvelopeOffset;
            mapLayer.IsHighDetail = (mapLayerDTO.baseFlags & (byte)LayerBaseFlags.IsHighDetail) != 0;

            TryCreate_v3(ref mapLayer, mapItemDTO, payload);
        }

        private void TryCreate_v3(ref MapTilesLayer mapLayer, IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            var mapLayerDTO = mapItemDTO as MapTilesLayerDTO_v3;

            if (mapLayerDTO == null)
                return;

            mapLayer.Name = mapLayerDTO.nameArray.IntsToStr();
        }
    }
}
