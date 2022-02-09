using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar.PropertiesBox;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Sidebar.PropertiesBox
{
    internal sealed partial class MapGameLayerPropertiesView : UserControl
    {
        private MapGameLayerPropertiesViewModel ViewModel { get; }

        private MapGameLayerPropertiesView()
        {
            this.InitializeComponent();
        }

        public MapGameLayerPropertiesView(MapGameLayerPropertiesViewModel viewModel) : this()
        {
            ViewModel = viewModel;
        }

        private void ShiftLayerLeftBtn_Click(object sender, RoutedEventArgs e) => ViewModel.ShiftLeft();

        private void ShiftLayerRightBtn_Click(object sender, RoutedEventArgs e) => ViewModel.ShiftRight();

        private void ShiftLayerTopBtn_Click(object sender, RoutedEventArgs e) => ViewModel.ShiftTop();

        private void ShiftLayerBottomBtn_Click(object sender, RoutedEventArgs e) => ViewModel.ShiftBottom();
    }
}
