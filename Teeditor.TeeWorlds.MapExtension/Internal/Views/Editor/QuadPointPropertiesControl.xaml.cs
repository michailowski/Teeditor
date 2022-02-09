using Microsoft.Toolkit.Uwp.UI.Controls;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Editor;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Editor
{
    internal sealed partial class QuadPointPropertiesControl : UserControl
    {
        public QuadPointPropertiesViewModel ViewModel { get; }

        public QuadPointPropertiesControl(QuadPointPropertiesViewModel viewModel)
        {
            this.InitializeComponent();

            ViewModel = viewModel;
        }

        private void AspectBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel.AspectRatio();

        private void CenterBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel.CenterPivot();

        private void SquareBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel.SquareQuad();

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel.RemoveQuad();

        private void ResetColorEnvelopeBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel.ResetColorEnvelope();

        private void ResetPositionEnvelopeBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel.ResetPositionEnvelope();

        private void ResetColorBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel.ResetColor();

        private async void PickColorBtn_Click(object sender, RoutedEventArgs e)
        {
            var eyedropper = new Eyedropper();

            var pickedColor = await eyedropper.Open();

            ViewModel.SetColor(pickedColor);
        }
    }
}
