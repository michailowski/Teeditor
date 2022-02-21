using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Teeditor.Common;
using Teeditor.Common.Utilities;
using Teeditor.Common.ViewModels;
using Teeditor.TeeWorlds.MapExtension.Internal.Views.Sidebar;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Models.Sidebar;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar
{
    internal class ImagesBoxViewModel : BoxViewModelBase
    {
        private Map _map;
        private ImagesContainer _imagesContainer;

        internal ImagesContainer ImagesContainer
        {
            get => _imagesContainer;
            set => Set(ref _imagesContainer, value);
        }

        public ImagesBoxViewModel()
        {
            Label = "Images";
            MenuText = "Images Box";
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["ImagesBoxIconPath"]) };
            DefaultDock = SidebarDock.Left;
            DefaultActive = false;
        }

        public override void SetTab(ITab tab)
        {
            _map = (Map)tab.Data;

            if (_map == null)
                return;

            ImagesContainer = _map.ImagesContainer;
        }

        internal async Task LoadImageFromFileAsync(StorageFile file)
        {
            var image = new MapImage();
            ImagesContainer.Add(image);

            await image.TryLoad(file);
        }

        internal async Task UpdateImageFromFileAsync(MapImage image, StorageFile file) => await image.TryLoad(file);

        internal async Task SaveImagesToFolderAsync(StorageFolder folder)
        {
            foreach (var image in ImagesContainer.Items)
            {
                await image.Save(folder);
            }
        }

        internal async Task SaveImageToFileAsync(MapImage image, StorageFile file) => await image.Save(file);

        internal void RemoveImage(MapImage image) => ImagesContainer.Remove(image);
    }
}
