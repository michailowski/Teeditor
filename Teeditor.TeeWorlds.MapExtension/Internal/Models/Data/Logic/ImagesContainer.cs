using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Factories;
using Teeditor_TeeWorlds_Direct3DInterop;
using Windows.Storage;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic
{
    internal class ImagesContainer
    {
        private ObservableCollection<MapImage> _items;
        private Dictionary<MapImage, int> _textureCarriersCount;
        private Dictionary<MapImage, int> _textureArrayCarriersCount;
        private MapImageFactory _imageFactory;

        public ReadOnlyObservableCollection<MapImage> Items { get; }
        public MapImage Entities { get; private set; }
        public TexturesManager TexturesManager { get; }

        public ImagesContainer()
        {
            _items = new ObservableCollection<MapImage>();
            _textureCarriersCount = new Dictionary<MapImage, int>();
            _textureArrayCarriersCount = new Dictionary<MapImage, int>();
            _imageFactory = new MapImageFactory();

            Items = new ReadOnlyObservableCollection<MapImage>(_items);

            var device = CanvasDevice.GetSharedDevice();

            using (var _lock = device.Lock())
            {
                TexturesManager = new TexturesManager(device);
            }

            InitEntities();
        }

        #region Logic

        public async void Add(StorageFile file)
        {
            var image = (MapImage)_imageFactory.Create();

            image.Loaded += Image_Loaded;
            image.CarrierChanged += Image_CarrierChanged;

            _items.Add(image);

            await image.TryLoad(file);
        }

        public async void Add(MapImage image)
        {
            image.Loaded += Image_Loaded;
            image.CarrierChanged += Image_CarrierChanged;

            _items.Add(image);

            await image.TryLoad();
        }

        public void Remove(MapImage image)
        {
            image.Loaded -= Image_Loaded;
            image.CarrierChanged -= Image_CarrierChanged;

            _items.Remove(image);
        }

        public bool TryGet(int index, out MapImage image)
        {
            if (index < 0 ||index >= _items.Count)
            {
                image = null;
                return false;
            }

            image = _items[index];
            return true;
        }

        private void Image_Loaded(object sender, EventArgs e)
        {
            var image = (MapImage)sender;

            // TODO: remove texture before from memory if we already have one
            image.TextureHandle = TexturesManager.AddTexture(image.Data, (uint)image.Width, (uint)image.Height);
            EnsureTextureLoading(image);
        }

        private void Image_CarrierChanged(object sender, ImageCarrierChangedEventArgs e)
        {
            var image = (MapImage)sender;

            if (image == null)
                return;

            if (e.NewCarrier != null)
            {
                if (e.NewCarrier is MapTilesLayer tilesLayer)
                {
                    tilesLayer.ImageId = Items.IndexOf(image);

                    _textureArrayCarriersCount.TryAdd(image, 0);
                    _textureArrayCarriersCount[image]++;
                }
                else if (e.NewCarrier is MapQuadsLayer quadsLayer)
                {
                    quadsLayer.ImageId = Items.IndexOf(image);

                    _textureCarriersCount.TryAdd(image, 0);
                    _textureCarriersCount[image]++;
                }
            }
            else if (e.OldCarrier != null)
            {
                if (e.OldCarrier is MapTilesLayer tilesLayer)
                {
                    tilesLayer.ImageId = -1;

                    if (_textureArrayCarriersCount.ContainsKey(image))
                        _textureArrayCarriersCount[image]--;
                }
                else if (e.OldCarrier is MapQuadsLayer quadsLayer)
                {
                    quadsLayer.ImageId = -1;

                    if (_textureCarriersCount.ContainsKey(image))
                        _textureCarriersCount[image]--;
                }
            }

            EnsureTextureLoading(image);
        }

        #endregion

        #region Entities

        private async void InitEntities()
        {
            Entities = new MapImage();
            Entities.Loaded += Entities_Loaded;

            var fileUri = new Uri("ms-appx:///Teeditor.TeeWorlds.MapExtension/Internal/Assets/GameEntities/" + Path.ChangeExtension("default", ".png"));
            var file = await StorageFile.GetFileFromApplicationUriAsync(fileUri);

            await Entities.TryLoad(file);
        }

        private void Entities_Loaded(object sender, EventArgs e)
        { 
            Entities.TextureHandle = TexturesManager.AddTexture(Entities.Data, (uint)Entities.Width, (uint)Entities.Height);
            _textureArrayCarriersCount.Add(Entities, 1);
            EnsureTextureLoading(Entities);
        }

        #endregion

        #region Texture Loading/Unloading

        private void EnsureTextureLoading(MapImage image)
        {
            if (image.TextureHandle == null)
                return;

            _textureCarriersCount.TryGetValue(image, out var textureCarriers);
            _textureArrayCarriersCount.TryGetValue(image, out var textureArrayCarriers);

            var device = CanvasDevice.GetSharedDevice();

            using (var _lock = device.Lock())
            {
                if (textureCarriers > 0 && TexturesManager.IsTextureLoaded(image.TextureHandle) == false)
                {
                    TexturesManager.LoadTexture(image.TextureHandle);
                }
                else if (textureArrayCarriers > 0 && TexturesManager.IsTextureArrayLoaded(image.TextureHandle) == false)
                {
                    TexturesManager.LoadTextureArray(image.TextureHandle);
                }
            }
        }

        private void EnsureTextureUnloading(MapImage image)
        {
            // TODO
        }

        #endregion
    }
}