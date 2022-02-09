using Teeditor.ViewModels;
using Windows.UI.Xaml;

namespace Teeditor.Common.Views.Toolbar
{
    internal sealed partial class CommandToolControl : ToolControl
    {
        private new CommandToolViewModel ViewModel => (CommandToolViewModel)_viewModel;

        internal CommandToolControl(CommandToolViewModel viewModel)
            : base(viewModel)
        {
            this.InitializeComponent();
        }

        private void UndoBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel?.Undo();

        private void RedoBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel?.Redo();
    }
}
