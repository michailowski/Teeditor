using Microsoft.Toolkit.Uwp.UI.Controls;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar.PropertiesBox;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Sidebar.PropertiesBox
{
    internal sealed partial class MapTilesLayerPropertiesView : UserControl
    {
        private MapTilesLayerPropertiesViewModel ViewModel { get; }
        private MapTilesLayerPropertiesView()
        {
            this.InitializeComponent();
        }

        public MapTilesLayerPropertiesView(MapTilesLayerPropertiesViewModel viewModel) : this()
        {
            DataContext = ViewModel = viewModel;
        }

        private void ResetColorBtn_Click(object sender, RoutedEventArgs e) => ViewModel.ResetColor();

        private void ResetImageBtn_Click(object sender, RoutedEventArgs e) => ViewModel.ResetImage();

        private void ResetColorEnvelopeBtn_Click(object sender, RoutedEventArgs e) => ViewModel.ResetColorEnvelope();

        private void ShiftLayerLeftBtn_Click(object sender, RoutedEventArgs e) => ViewModel.ShiftLeft();

        private void ShiftLayerRightBtn_Click(object sender, RoutedEventArgs e) => ViewModel.ShiftRight();

        private void ShiftLayerTopBtn_Click(object sender, RoutedEventArgs e) => ViewModel.ShiftTop();

        private void ShiftLayerBottomBtn_Click(object sender, RoutedEventArgs e) => ViewModel.ShiftBottom();

        private async void PickColorBtn_Click(object sender, RoutedEventArgs e)
        {
            var eyedropper = new Eyedropper();

            ViewModel.Color = await eyedropper.Open();
        }
    }
}
