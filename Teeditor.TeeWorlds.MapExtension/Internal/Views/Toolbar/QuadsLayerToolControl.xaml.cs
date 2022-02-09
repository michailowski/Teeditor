using Teeditor.Common.Views.Toolbar;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Toolbar;
using Windows.UI.Xaml;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Toolbar
{
    internal sealed partial class QuadsLayerToolControl : ToolControl
    {
        private new QuadsLayerToolViewModel ViewModel => (QuadsLayerToolViewModel) _viewModel;

        public QuadsLayerToolControl(QuadsLayerToolViewModel viewModel)
            : base(viewModel)
        {
            this.InitializeComponent();
        }

        private void AddQuadBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel.AddQuad();
    }
}
