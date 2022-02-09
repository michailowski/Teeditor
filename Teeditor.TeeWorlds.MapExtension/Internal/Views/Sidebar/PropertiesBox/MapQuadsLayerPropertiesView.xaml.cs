using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar.PropertiesBox;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Sidebar.PropertiesBox
{
    internal sealed partial class MapQuadsLayerPropertiesView : UserControl
    {
        private MapQuadsLayerPropertiesViewModel ViewModel { get; }
        private MapQuadsLayerPropertiesView()
        {
            this.InitializeComponent();
        }

        public MapQuadsLayerPropertiesView(MapQuadsLayerPropertiesViewModel viewModel) : this()
        {
            DataContext = ViewModel = viewModel;
        }

        private void ResetImageBtn_Click(object sender, RoutedEventArgs e) => ViewModel.ResetImage();
    }
}
