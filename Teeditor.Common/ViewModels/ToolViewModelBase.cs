using Teeditor.Common.Models.Tab;
using Teeditor.Common.Utilities;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Common.ViewModels
{
    public abstract class ToolViewModelBase : DynamicViewModel, IToolViewModel
    {
        public string Label { get; protected set; }
        public string MenuText { get; protected set; }
        public PathIcon MenuIcon { get; protected set; }

        public ToolViewModelBase()
        {
            Label = "Default label";
            MenuText = "Default menu text";

            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["ExplorerBoxIconPath"]) };
        }

        public abstract void SetTab(ITab tab);
    }
}
