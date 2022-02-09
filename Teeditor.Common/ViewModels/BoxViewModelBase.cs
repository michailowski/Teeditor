using Teeditor.Common.Models.Sidebar;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Utilities;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Common.ViewModels
{
    public abstract class BoxViewModelBase : DynamicViewModel, IBoxViewModel
    {
        public string Label { get; protected set; }

        public string MenuText { get; protected set; }

        public PathIcon MenuIcon { get; protected set; }

        public SidebarDock Dock { get; set; }

        public BoxViewModelBase()
        {
            Label = "Default label";
            MenuText = "Default menu text";
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["ExplorerBoxIconPath"]) };
            Dock = SidebarDock.Left;
        }

        public abstract void SetTab(ITab tab);
    }
}
