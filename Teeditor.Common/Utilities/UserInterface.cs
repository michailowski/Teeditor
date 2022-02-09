 using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace Teeditor.Common.Utilities
{
    public static class UserInterface
    {
        public static Path PathMarkupToGraphPath(string pathMarkup)
        {
            var xaml =
                "<Path xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' Stroke='White' StrokeThickness='2'>" +
                $"<Path.Data>{pathMarkup}</Path.Data>" + "</Path>";
            return XamlReader.Load(xaml) as Path;
        }

        public static Geometry PathMarkupToGeometry(string pathMarkup)
        {
            var geometry = (Geometry)XamlReader.Load(
                "<Geometry xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>"
                + pathMarkup + "</Geometry>");

            return geometry;
        }

        public static void SetIconPathMarkup(this Button btn, string pathMarkup)
        {
            btn.Content = PathMarkupToPathIcon(pathMarkup);
        }

        private static PathIcon PathMarkupToPathIcon(string pathMarkup)
        {
            string xaml = String.Format("<PathIcon xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'><PathIcon.Data>{0}</PathIcon.Data></PathIcon>", pathMarkup);
            var pathIcon = XamlReader.Load(xaml) as PathIcon;

            return pathIcon;
        }

        public static async void SetCursorType(CoreCursorType type)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Window.Current.CoreWindow.PointerCursor.Type != type)
                    Window.Current.CoreWindow.PointerCursor = new CoreCursor(type, 0);
            });
        }

        public static async Task<SoftwareBitmap> GetUIBitmapAsync(UIElement element, byte opacity)
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(element);

            var buffer = await renderTargetBitmap.GetPixelsAsync();

            DataReader dataReader = DataReader.FromBuffer(buffer);
            byte[] bytes = new byte[buffer.Length];
            dataReader.ReadBytes(bytes);

            for (int i = 0; i < bytes.Length; i += 4)
            {
                int alpha = bytes[i + 3] - (255 - opacity);
                bytes[i + 3] = (byte)Math.Clamp(alpha, 0, 255);
            }

            var newBuffer = bytes.AsBuffer();

            var bitmap = SoftwareBitmap.CreateCopyFromBuffer(newBuffer,
                BitmapPixelFormat.Bgra8,
                renderTargetBitmap.PixelWidth,
                renderTargetBitmap.PixelHeight,
                BitmapAlphaMode.Premultiplied);

            return bitmap;
        }
    }
}
