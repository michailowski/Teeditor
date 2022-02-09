using Teeditor.ViewModels.Dialogs;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Views.Dialogs
{
    public sealed partial class ProjectCreateDialog : ContentDialog
    {
        public ProjectCreateDialogViewModel ViewModel { get; }

        public ProjectCreateDialog(ProjectCreateDialogViewModel viewModel)
        {
            this.InitializeComponent();

            ViewModel = viewModel;
        }
    }
}
