using System;
using System.Numerics;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload;
using Windows.UI;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Factories
{
    internal class MapFactory
    {
        private MapInfoFactory _infoFactory;
        private MapImageFactory _imageFactory;
        private MapGroupFactory _groupFactory;
        private MapTilesLayerFactory _tilesLayerFactory;
        private MapQuadsLayerFactory _quadsLayerFactory;
        private MapEnvelopeFactory _envelopeFactory;

        private const int TotalPartsNumber = 3;
        private int _loadedTotalPartsCount = 0;

        private int _imagesNumber = 0;
        private int _loadedImagesCount;
        private Action ProcessImagesTracking;

        public MapFactory()
        {
            _infoFactory = new MapInfoFactory();
            _imageFactory = new MapImageFactory();
            _groupFactory = new MapGroupFactory();
            _tilesLayerFactory = new MapTilesLayerFactory();
            _quadsLayerFactory = new MapQuadsLayerFactory();
            _envelopeFactory = new MapEnvelopeFactory();
        }

        public Map Create(MapFilePayload payload)
        {
            if (payload.Type == PayloadType.Creating)
            {
                return Create();
            }

            var info = GetInfo(payload);
            var map = new Map(info);

            AppendImages(map, payload);
            AppendEnvelopes(map, payload);
            AppendGroupedLayers(map, payload);

            return map;
        }

        private Map Create()
        {
            var info = (MapInfo)_infoFactory.Create();
            var map = new Map(info);

            var commonGroup = (MapGroup)_groupFactory.Create();
            commonGroup.Parallax = Vector2.Zero;
            
            var quadsLayer = (MapQuadsLayer)_quadsLayerFactory.Create();

            var backgroundQuad = new MapQuad();
            int quadWidth = 800;
            int quadHeight = 600;

            backgroundQuad.Points[0].PositionX = backgroundQuad.Points[2].PositionX = -quadWidth;
            backgroundQuad.Points[1].PositionX = backgroundQuad.Points[3].PositionX = quadWidth;
            backgroundQuad.Points[0].PositionY = backgroundQuad.Points[1].PositionY = -quadHeight;
            backgroundQuad.Points[2].PositionY = backgroundQuad.Points[3].PositionY = quadHeight;
            backgroundQuad.Points[4].PositionX = backgroundQuad.Points[4].PositionY = 0;

            backgroundQuad.Points[0].Color = Color.FromArgb(255, 94, 132, 174);
            backgroundQuad.Points[1].Color = backgroundQuad.Points[0].Color;

            backgroundQuad.Points[2].Color = Color.FromArgb(255, 204, 232, 255);
            backgroundQuad.Points[3].Color = backgroundQuad.Points[2].Color;

            quadsLayer.Quads.Add(backgroundQuad);

            var gameGroup = (MapGroup)_groupFactory.Create();
            var gameLayer = _tilesLayerFactory.CreateGameLayer();
            gameLayer.Image = map.ImagesContainer.Entities;

            commonGroup.Add(quadsLayer);
            gameGroup.Add(gameLayer);

            map.GroupedLayersContainer.Add(commonGroup);
            map.GroupedLayersContainer.Add(gameGroup);

            for (int i = 0; i < TotalPartsNumber; i++)
            {
                TotalLoadingTracking(map);
            }

            return map;
        }

        private MapInfo GetInfo(MapFilePayload payload)
            => (MapInfo)_infoFactory.Create(payload.Items.InfoDTO, payload);

        private void AppendImages(Map map, MapFilePayload payload)
        {
            if (payload.Items.ImageDTOs.Count == 0)
            {
                TotalLoadingTracking(map);
                return;
            }

            _imagesNumber = payload.Items.ImageDTOs.Count;

            ProcessImagesTracking = () => ImagesLoadingTracking(map);

            foreach (var imageDTO in payload.Items.ImageDTOs)
            {
                var image = (MapImage)_imageFactory.Create(imageDTO, payload);
                image.Loaded += Image_Loaded;

                map.ImagesContainer.Add(image);
            }
        }

        private void Image_Loaded(object sender, EventArgs e)
            => ProcessImagesTracking();

        private void AppendEnvelopes(Map map, MapFilePayload payload)
        {
            foreach (MapEnvelopeDTO_v1 envelopeDTO in payload.Items.EnvelopeDTOs)
            {
                var envelope = (MapEnvelope)_envelopeFactory.Create(envelopeDTO, payload);

                for (int i = envelopeDTO.startPointIndex; i < envelopeDTO.startPointIndex + envelopeDTO.pointsNumber; i++)
                {
                    var envelopePointDTO = payload.Items.EnvelopePointDTOs[i];

                    if (envelopePointDTO is MapEnvelopePointDTO_v1)
                    {
                        var point = (MapEnvelopePoint)envelope.PointFactory.Create(envelopePointDTO, payload);

                        envelope.Points.Add(point);
                    }
                }

                map.EnvelopesContainer.Add(envelope);
            }

            TotalLoadingTracking(map);
        }

        private void AppendGroupedLayers(Map map, MapFilePayload payload)
        {
            foreach (MapGroupDTO_v1 groupDTO in payload.Items.GroupDTOs)
            {
                var group = (MapGroup)_groupFactory.Create(groupDTO, payload);

                for (int i = groupDTO.startLayerIndex; i < groupDTO.startLayerIndex + groupDTO.layersNumber; i++)
                {
                    if (payload.Items.LayerDTOs[i] is MapTilesLayerDTO_v1 tilesLayerDTO)
                    {
                        var layer = (MapTilesLayer)_tilesLayerFactory.Create(tilesLayerDTO, payload);

                        if (layer.IsGameLayer)
                        {
                            layer.Image = map.ImagesContainer.Entities;
                        }
                        else
                        {
                            map.ImagesContainer.TryGet(tilesLayerDTO.imageId, out var layerImage);
                            layer.Image = layerImage;
                        }

                        map.EnvelopesContainer.TryGet(tilesLayerDTO.colorEnvelopeId, out var layerColorEnvelope);
                        layer.ColorEnvelope = layerColorEnvelope;

                        group.Add(layer);
                    }
                    else if (payload.Items.LayerDTOs[i] is MapQuadsLayerDTO_v1 quadsLayerDTO)
                    {
                        var layer = (MapQuadsLayer)_quadsLayerFactory.Create(payload.Items.LayerDTOs[i], payload);

                        map.ImagesContainer.TryGet(quadsLayerDTO.imageId, out var layerImage);
                        layer.Image = layerImage;

                        group.Add(layer);
                    }
                }

                map.GroupedLayersContainer.Add(group);
            }

            TotalLoadingTracking(map);
        }

        private void TotalLoadingTracking(Map map)
        {
            _loadedTotalPartsCount++;

            if (_loadedTotalPartsCount >= TotalPartsNumber)
            {
                map.MarkAsLoaded();
                _loadedTotalPartsCount = 0;
            }
        }

        private void ImagesLoadingTracking(Map map)
        {
            _loadedImagesCount++;

            if (_loadedImagesCount >= _imagesNumber)
            {
                TotalLoadingTracking(map);
                _loadedImagesCount = 0;
                _imagesNumber = 0;
            }
        }
    }
}