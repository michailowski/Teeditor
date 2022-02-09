using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Dialogs;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Dialogs
{
    internal sealed partial class AddBookmarkDialog : ContentDialog
    {
        private AddBookmarkDialogViewModel _viewModel;

        public AddBookmarkDialog(AddBookmarkDialogViewModel viewModel)
        {
            this.InitializeComponent();

            _viewModel = viewModel;
            SetDefaultBookmarkLabel();
        }

        private void SetDefaultBookmarkLabel()
            => BookmarkLabel.Text = _viewModel?.DefaultLabel;
        
        private void BookmarkAddBtn_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
            => _viewModel?.TryAddBookmark(BookmarkLabel.Text);
    }
}
