using Teeditor.Common.Views.Toolbar;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Toolbar;
using Windows.UI.Xaml;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Toolbar
{
    internal sealed partial class ViewToolControl : ToolControl
    {
        private new ViewToolViewModel ViewModel => (ViewToolViewModel) _viewModel;

        public ViewToolControl(ViewToolViewModel viewModel)
            : base(viewModel)
        {
            this.InitializeComponent();
        }
    }
}
