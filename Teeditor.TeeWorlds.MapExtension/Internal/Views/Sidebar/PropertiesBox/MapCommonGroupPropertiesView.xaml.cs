using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar.PropertiesBox;
using Windows.UI.Xaml.Controls;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Sidebar.PropertiesBox
{
    internal sealed partial class MapCommonGroupPropertiesView : UserControl
    {
        private MapCommonGroupPropertiesViewModel ViewModel { get; }

        private MapCommonGroupPropertiesView()
        {
            this.InitializeComponent();
        }

        public MapCommonGroupPropertiesView(MapCommonGroupPropertiesViewModel viewModel) : this()
        {
            ViewModel = viewModel;
        }
    }
}
