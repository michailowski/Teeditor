using Teeditor.Common.Models.Tab;
using Teeditor.Common.Utilities;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Common.ViewModels
{
    public abstract class PanelViewModelBase : DynamicViewModel, IPanelViewModel
    {
        public string Label { get; protected set; }
        public string MenuText { get; protected set; }
        public PathIcon MenuIcon { get; protected set; }

        public PanelViewModelBase()
        {
            Label = "Default label";
            MenuText = "Default menu text";
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["ExplorerBoxIconPath"]) };
        }

        public abstract void SetTab(ITab tab);
    }
}
