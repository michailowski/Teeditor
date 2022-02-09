using System;
using System.Linq;
using Teeditor.Common.Views.Toolbar;
using Teeditor.Common.Utilities;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Toolbar;
using Teeditor.TeeWorlds.MapExtension.Internal.Views.Dialogs;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Toolbar
{
    internal sealed partial class CameraToolControl : ToolControl
    {
        private new CameraToolViewModel ViewModel => (CameraToolViewModel) _viewModel;

        public CameraToolControl(CameraToolViewModel viewModel)
            : base(viewModel)
        {
            this.InitializeComponent();
        }

        private void ResetCameraPositionBtn_Click(object sender, RoutedEventArgs e) 
            => ViewModel.ResetPosition();

        private void ResetCameraZoomBtn_Click(object sender, RoutedEventArgs e) 
            => ViewModel.ResetZoom();

        private void ShowBookmarksMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            var menu = new MenuFlyout();

            var item = new MenuFlyoutItem
            {
                Text = "Add Bookmark",
                Icon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["AddBookmarkMiniIconPath"]) }
            };

            item.Click += delegate
            {
                ShowBookmarkAddingDialog();
            };

            menu.Items.Add(item);

            if (ViewModel.Bookmarks.Any())
            {
                menu.Items.Add(new MenuFlyoutSeparator());
            }

            foreach (var bookmark in ViewModel.Bookmarks)
            {
                var bookmarkItem = new MenuFlyoutItem
                {
                    Text = bookmark.Value
                };

                bookmarkItem.Click += delegate
                {
                    ViewModel.GoTo(bookmark);
                };

                menu.Items.Add(bookmarkItem);
            }

            var btn = (Button)sender;

            menu.Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft;
            menu.ShowAt(btn);
        }

        private async void ShowBookmarkAddingDialog()
        {
            var addBookmarkDialog = new AddBookmarkDialog(ViewModel.AddBookmarkDialogViewModel);

            await addBookmarkDialog.ShowAsync();
        }

        private void CameraZoomInBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel.ZoomIn();

        private void CameraZoomOutBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel.ZoomOut();
    }
}
