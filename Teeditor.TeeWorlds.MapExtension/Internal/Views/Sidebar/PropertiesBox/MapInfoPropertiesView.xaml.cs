using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar.PropertiesBox;
using Windows.UI.Xaml.Controls;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Sidebar.PropertiesBox
{
    internal sealed partial class MapInfoPropertiesView : UserControl
    {
        private MapInfoPropertiesViewModel ViewModel { get; }

        private MapInfoPropertiesView()
        {
            this.InitializeComponent();
        }

        public MapInfoPropertiesView(MapInfoPropertiesViewModel viewModel) : this()
        {
            ViewModel = viewModel;
        }
    }
}
