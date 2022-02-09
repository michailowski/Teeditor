using Teeditor.Common.Views.Toolbar;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Toolbar;
using Windows.UI.Xaml;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Toolbar
{
    internal sealed partial class LayerSelectionToolControl : ToolControl
    {
        private new LayerSelectionToolViewModel ViewModel => (LayerSelectionToolViewModel) _viewModel;

        public LayerSelectionToolControl(LayerSelectionToolViewModel viewModel)
            : base(viewModel)
        {
            this.InitializeComponent();
        }

        private void FlipVerticalBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel.FlipVertical();

        private void FlipHorizontalBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel.FlipHorizontal();

        private void RotateClockwiseBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel.RotateClockwise();

        private void RotateCounterClockwiseBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel.RotateCounterClockwise();
    }
}
