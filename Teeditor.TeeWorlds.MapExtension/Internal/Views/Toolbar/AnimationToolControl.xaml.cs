using Teeditor.Common.Views.Toolbar;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Toolbar;
using Windows.UI.Xaml;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Toolbar
{
    internal sealed partial class AnimationToolControl : ToolControl
    {
        private new AnimationToolViewModel ViewModel => (AnimationToolViewModel) _viewModel;

        public AnimationToolControl(AnimationToolViewModel viewModel)
            : base(viewModel)
        {
            this.InitializeComponent();
        }

        private void ResetTimerBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel?.ResetTimer();
    }
}
