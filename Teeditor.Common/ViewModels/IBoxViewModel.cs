using Teeditor.Common.Models.Sidebar;
using Teeditor.Common.Models.Tab;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Common.ViewModels
{
    public interface IBoxViewModel
    {
        string Label { get; }
        string MenuText { get; }
        PathIcon MenuIcon { get; }
        SidebarDock Dock { get; }

        void SetTab(ITab tab);
    }
}
