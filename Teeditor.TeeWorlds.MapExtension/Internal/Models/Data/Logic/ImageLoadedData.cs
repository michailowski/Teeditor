using Microsoft.Graphics.Canvas;
using Windows.UI.Xaml.Media.Imaging;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic
{
    internal struct ImageLoadedData
    {
        public string Name { get; set; }
        public SoftwareBitmapSource ThumbnailBitmap { get; set; }
        public CanvasBitmap CanvasBitmap { get; set; }
        public byte[] Data { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsExternal { get; set; }
        public bool IsEmpty => CanvasBitmap == null || ThumbnailBitmap == null;
        public static ImageLoadedData Empty { get; }
    }
}
