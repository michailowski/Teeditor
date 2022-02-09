using Microsoft.Graphics.Canvas;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic
{
    internal static class ImageLoader
    {
        public static async Task<ImageLoadedData> LoadAsync(string name)
        {
            if (name == null)
                return ImageLoadedData.Empty;

            try
            {
                var installedFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                var mapResourcesFolder = await installedFolder.GetFolderAsync(@"Internal\Assets\DefaultTilesets");

                var file = await mapResourcesFolder?.TryGetItemAsync(name + ".png") as StorageFile;

                if (file == null)
                    return ImageLoadedData.Empty;

                var device = CanvasDevice.GetSharedDevice();

                ImageLoadedData loadedData;

                using (IRandomAccessStream stream = await file.OpenReadAsync())
                {
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                    if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
                    {
                        softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                    }

                    var thumbnailBitmap = new SoftwareBitmapSource();
                    await thumbnailBitmap.SetBitmapAsync(softwareBitmap);

                    var buffer = new Windows.Storage.Streams.Buffer((uint)softwareBitmap.PixelWidth * (uint)softwareBitmap.PixelHeight * 4);
                    softwareBitmap.CopyToBuffer(buffer);

                    loadedData = new ImageLoadedData()
                    {
                        ThumbnailBitmap = thumbnailBitmap,
                        CanvasBitmap = CanvasBitmap.CreateFromSoftwareBitmap(device, softwareBitmap),
                        Data = buffer.ToArray(),
                        Width = softwareBitmap.PixelWidth,
                        Height = softwareBitmap.PixelHeight,
                        IsExternal = true
                    };
                }

                return loadedData;
            }
            catch
            {
                return ImageLoadedData.Empty;
            }
        }

        public static async Task<ImageLoadedData> LoadAsync(byte[] data, int width, int height)
        {
            try
            {
                var softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(data.AsBuffer(), BitmapPixelFormat.Rgba8, width, height, BitmapAlphaMode.Straight);
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                var thumbnailBitmap = new SoftwareBitmapSource();
                await thumbnailBitmap.SetBitmapAsync(softwareBitmap);

                var buffer = new Windows.Storage.Streams.Buffer((uint)softwareBitmap.PixelWidth * (uint)softwareBitmap.PixelHeight * 4);
                softwareBitmap.CopyToBuffer(buffer);

                var device = CanvasDevice.GetSharedDevice();

                var loadedData = new ImageLoadedData()
                {
                    ThumbnailBitmap = thumbnailBitmap,
                    CanvasBitmap = CanvasBitmap.CreateFromSoftwareBitmap(device, softwareBitmap),
                    Data = buffer.ToArray(),
                    Width = softwareBitmap.PixelWidth,
                    Height = softwareBitmap.PixelHeight,
                    IsExternal = false
                };

                return loadedData;
            }
            catch
            {
                return ImageLoadedData.Empty;
            }
        }

        public static async Task<ImageLoadedData> LoadAsync(StorageFile file)
        {
            var filePath = file.Path;

            try
            {
                var device = CanvasDevice.GetSharedDevice();
                ImageLoadedData loadedData;

                using (IRandomAccessStream stream = await file.OpenReadAsync())
                {
                    var decoder = await BitmapDecoder.CreateAsync(stream);
                    var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                    if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
                    {
                        softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                    }

                    var thumbnailBitmap = new SoftwareBitmapSource();
                    await thumbnailBitmap.SetBitmapAsync(softwareBitmap);

                    var buffer = new Windows.Storage.Streams.Buffer((uint)softwareBitmap.PixelWidth * (uint)softwareBitmap.PixelHeight * 4);
                    softwareBitmap.CopyToBuffer(buffer);

                    var fileProperties = await file.GetBasicPropertiesAsync();
                    var isExternal = await ImageLocallyExistsAsync(new Uri(filePath), fileProperties.Size);

                    loadedData = new ImageLoadedData()
                    {
                        Name = Path.GetFileNameWithoutExtension(filePath),
                        ThumbnailBitmap = thumbnailBitmap,
                        CanvasBitmap = CanvasBitmap.CreateFromSoftwareBitmap(device, softwareBitmap),
                        Data = buffer.ToArray(),
                        Width = softwareBitmap.PixelWidth,
                        Height = softwareBitmap.PixelHeight,
                        IsExternal = isExternal
                    };
                }

                return loadedData;
            }
            catch
            {
                return ImageLoadedData.Empty;
            }
        }

        private static async Task<bool> ImageLocallyExistsAsync(Uri fileUri, ulong fileSize)
        {
            var installedFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var mapResourcesFolder = await installedFolder?.GetFolderAsync(@"Internal\Assets\DefaultTilesets");
            var fileName = Path.GetFileName(fileUri.ToString());

            if (string.IsNullOrEmpty(fileName))
                return false;

            var localFile = await mapResourcesFolder?.TryGetItemAsync(fileName) as StorageFile;

            if (localFile == null)
                return false;

            var localFileProperties = await localFile.GetBasicPropertiesAsync();

            return localFileProperties.Size == fileSize;
        }

        private static Uri GetFileUri(string name)
            => new Uri("ms-appx:///Internal/Assets/DefaultTilesets/" + Path.ChangeExtension(name, ".png"));
    }
}
