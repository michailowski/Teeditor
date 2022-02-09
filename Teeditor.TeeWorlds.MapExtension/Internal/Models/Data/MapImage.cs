using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Graphics.Canvas;
using Teeditor_TeeWorlds_Direct3DInterop;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data
{
    internal class MapImage : MapItem
    {
        private string _name;
        private byte[] _data;
        private int _width;
        private int _height;
        private int _format;
        private bool _isExternal = true;
        private SoftwareBitmapSource _thumbnailBitmap;

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public byte[] Data 
        { 
            get => _data;
            set => Set(ref _data, value);
        }

        public int Width
        {
            get => _width;
            set => Set(ref _width, value);
        }

        public int Height
        {
            get => _height;
            set => Set(ref _height, value);
        }

        public int Format
        {
            get => _format;
            set => Set(ref _format, value);
        }

        public bool IsExternal
        {
            get => _isExternal;
            set
            {
                Set(ref _isExternal, value);
                OnPropertyChanged("ImageLocation");
            }
        }

        public string ImageLocation => IsExternal ? "External" : "Embedded";

        public TextureHandle TextureHandle { get; set; }

        public CanvasBitmap CanvasBitmap { get; private set; }

        public SoftwareBitmapSource ThumbnailBitmap
        {
            get => _thumbnailBitmap;
            set => Set(ref _thumbnailBitmap, value);
        }

        public event EventHandler Loaded;
        public event EventHandler<ImageCarrierChangedEventArgs> CarrierChanged;

        public void AddCarrier(MapLayer layer)
            => CarrierChanged?.Invoke(this, new ImageCarrierChangedEventArgs(layer, null));

        public void RemoveCarrier(MapLayer layer)
            => CarrierChanged?.Invoke(this, new ImageCarrierChangedEventArgs(null, layer));

        #region Saving

        public async Task Save(StorageFolder folder)
        {
            var file = await folder.CreateFileAsync($"{Name}.png", CreationCollisionOption.ReplaceExisting);

            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await CanvasBitmap.SaveAsync(fileStream, CanvasBitmapFileFormat.Png);
            }
        }

        public async Task Save(StorageFile file)
        {
            using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await CanvasBitmap.SaveAsync(fileStream, CanvasBitmapFileFormat.Png);
            }
        }

        #endregion

        #region Loading

        public async Task<bool> TryLoad()
        {
            bool loaded = IsExternal ? await TryLoadExternalAsync() : await TryLoadEmbeddedAsync();

            if (loaded == false)
                return false;

            Loaded?.Invoke(this, EventArgs.Empty);
            
            return true;
        }

        public async Task<bool> TryLoad(StorageFile file)
        {
            var isReloading = Data != null;
            var loadedData = await ImageLoader.LoadAsync(file);

            if (loadedData.IsEmpty)
                return false;

            Name = loadedData.Name;
            ThumbnailBitmap = loadedData.ThumbnailBitmap;
            CanvasBitmap = loadedData.CanvasBitmap;
            Data = loadedData.Data;
            Width = loadedData.Width;
            Height = loadedData.Height;
            IsExternal = loadedData.IsExternal;

            Loaded?.Invoke(this, EventArgs.Empty);

            if (isReloading)
                RaiseModification();

            return true;
        }

        private async Task<bool> TryLoadEmbeddedAsync()
        {
            var loadedData = await ImageLoader.LoadAsync(_data, _width, _height);

            if (loadedData.IsEmpty)
                return false;

            ThumbnailBitmap = loadedData.ThumbnailBitmap;
            CanvasBitmap = loadedData.CanvasBitmap;
            Data = loadedData.Data;

            return true;
        }

        private async Task<bool> TryLoadExternalAsync()
        {
            var loadedData = await ImageLoader.LoadAsync(_name);

            if (loadedData.IsEmpty)
                return false;

            ThumbnailBitmap = loadedData.ThumbnailBitmap;
            CanvasBitmap = loadedData.CanvasBitmap;
            Data = loadedData.Data;
            Width = loadedData.Width;
            Height = loadedData.Height;

            return true;
        }

        #endregion
    }
}
