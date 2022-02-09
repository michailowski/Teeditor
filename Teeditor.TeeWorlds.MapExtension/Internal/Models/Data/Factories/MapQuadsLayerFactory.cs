using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Factories
{
    internal class MapQuadsLayerFactory : MapItemFactory
    {
        public override MapItem Create()
        {
            var mapLayer = new MapQuadsLayer();

            return mapLayer;
        }

        public override MapItem Create(IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            MapQuadsLayer mapLayer = null;

            TryCreate_v1(ref mapLayer, mapItemDTO, payload);

            return mapLayer;
        }

        private void TryCreate_v1(ref MapQuadsLayer mapLayer, IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            var mapLayerDTO = mapItemDTO as MapQuadsLayerDTO_v1;

            if (mapLayerDTO == null)
                return;

            payload.Data.TryGetDecompressed(mapLayerDTO.quadsDataIndex, out byte[] quadsData);

            var quads = QuadsPacker.Unpack(quadsData);

            mapLayer = new MapQuadsLayer(quads);

            mapLayer.IsHighDetail = (mapLayerDTO.baseFlags & (byte)LayerBaseFlags.IsHighDetail) != 0;

            TryCreate_v2(ref mapLayer, mapItemDTO, payload);
        }

        private void TryCreate_v2(ref MapQuadsLayer mapLayer, IMapItemDTO mapItemDTO, MapFilePayload payload)
        {
            var mapLayerDTO = mapItemDTO as MapQuadsLayerDTO;

            if (mapLayerDTO == null)
                return;

            mapLayer.Name = mapLayerDTO.nameArray.IntsToStr();
        }
    }
}
