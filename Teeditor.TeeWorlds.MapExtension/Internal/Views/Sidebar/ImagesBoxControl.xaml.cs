using System;
using System.Collections.Generic;
using Teeditor.Common.Views.Sidebar;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Sidebar
{
    internal sealed partial class ImagesBoxControl : BoxControl
    {
        private new ImagesBoxViewModel ViewModel => (ImagesBoxViewModel)_viewModel;

        public ImagesBoxControl(ImagesBoxViewModel viewModel)
            : base(viewModel)
        {
            this.InitializeComponent();
        }

        private async void ImportImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var button = (MenuFlyoutItem)sender;
            
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            picker.FileTypeFilter.Add(".png");

            var file = await picker.PickSingleFileAsync();

            if (file == null)
                return;

            await ViewModel.LoadImageFromFileAsync(file);
        }

        private async void ExportAllImagesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var button = (MenuFlyoutItem)sender;

            var picker = new FolderPicker()
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            picker.FileTypeFilter.Add("*");

            var folder = await picker.PickSingleFolderAsync();

            if (folder == null)
                return;

            await ViewModel.SaveImagesToFolderAsync(folder);
        }

        private async void ExportImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var button = (MenuFlyoutItem)sender;
            var image = (MapImage)button.DataContext;

            var picker = new FileSavePicker()
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = image.Name
            };

            picker.FileTypeChoices.Add("Portable Network Graphics", new List<string>() { ".png" });

            var file = await picker.PickSaveFileAsync();

            if (file == null)
                return;

            await ViewModel.SaveImageToFileAsync(image, file);
        }

        private async void ReplaceImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var button = (MenuFlyoutItem)sender;
            var image = (MapImage)button.DataContext;

            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            picker.FileTypeFilter.Add(".png");

            var file = await picker.PickSingleFileAsync();

            if (file == null)
                return;
            
            await ViewModel.UpdateImageFromFileAsync(image, file);
        }

        private async void RemoveImageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var button = (MenuFlyoutItem)sender;
            var image = (MapImage)button.DataContext;
            
            var dialog = new ContentDialog
            {
                Title = "Warning",
                PrimaryButtonText = "Remove",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                Content = "All bindings to this image will also be deleted. Do you want to continue anyway?"
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ViewModel.RemoveImage(image);
            }
        }

        protected override IList<MenuFlyoutItemBase> GetMenuFlyoutItems()
        {
            return BoxMenuFlyout.Items;
        }
    }
}
