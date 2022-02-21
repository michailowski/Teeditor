using Teeditor.Common.Models.Sidebar;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Utilities;
using Teeditor.Common.ViewModels;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar
{
    internal class PropertiesBoxViewModel : BoxViewModelBase
    {
        private SidebarManager _sidebarManager;
        private Map _map;

        public MapItem PropertyBoxItem => _sidebarManager?.PropertyBoxItem;
        public Map Map => _map;

        public PropertiesBoxViewModel()
        {
            Label = "Properties";
            MenuText = "Properties Box";
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["PropertiesBoxIconPath"]) };
            DefaultDock = SidebarDock.Right;
            DefaultActive = false;
        }

        public override void SetTab(ITab tab)
        {
            _sidebarManager = tab?.SidebarManager as SidebarManager;
            _map = tab?.Data as Map;

            // DynamicViewModelBase need this setting for using of binding to model with dynamic properties
            DynamicModel = _sidebarManager;
        }

        public void ResetPropertyBoxItem() => _sidebarManager.PropertyBoxItem = null;
    }
}
