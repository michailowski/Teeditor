using Teeditor.TeeWorlds.MapExtension.Internal.Views.Sidebar;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.Common.Models.Sidebar;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic
{
    internal class SidebarManager : SidebarManagerBase
    {
        private MapItem _propertyBoxItem;

        public MapItem PropertyBoxItem
        {
            get => _propertyBoxItem;
            set => Set(ref _propertyBoxItem, value);
        }

        public SidebarManager()
        {
            var explorerViewModel = new ExplorerBoxViewModel();
            Items.Add(new ExplorerBoxControl(explorerViewModel));

            var navigatorViewModel = new NavigatorBoxViewModel();
            Items.Add(new NavigatorBoxControl(navigatorViewModel));

            var imagesViewModel = new ImagesBoxViewModel();
            Items.Add(new ImagesBoxControl(imagesViewModel));

            var propertiesViewModel = new PropertiesBoxViewModel();
            Items.Add(new PropertiesBoxControl(propertiesViewModel));

            var historyViewModel = new HistoryBoxViewModel();
            Items.Add(new HistoryBoxControl(historyViewModel));
        }
    }
}
