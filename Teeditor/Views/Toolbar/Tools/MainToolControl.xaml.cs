using System;
using Teeditor.Views.Dialogs;
using Teeditor.ViewModels.Toolbar.Tools;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Teeditor.Models;
using System.Collections.Generic;
using Windows.Storage;
using System.Threading.Tasks;

namespace Teeditor.Views.Toolbar.Tools
{
    internal sealed partial class MainToolControl : UserControl
    {
        private ProjectCreateDialog _projectCreateDialog;
        public MainToolViewModel ViewModel { get; }

        internal MainToolControl(MainToolViewModel viewModel)
        {
            this.InitializeComponent();

            ViewModel = viewModel;

            _projectCreateDialog = new ProjectCreateDialog(ViewModel.CreateProjectDialogViewModel);
        }

        private async void CreateProjectBtn_Click(object sender, RoutedEventArgs e)
        {
            ContentDialogResult result = await _projectCreateDialog.ShowAsync();

            var projectType = _projectCreateDialog.ViewModel.SelectedItem;

            _projectCreateDialog.ViewModel.SelectedItem = null;

            if (result != ContentDialogResult.Primary || projectType == null)
                return;

            ViewModel.CreateProject(projectType);
        }

        private async void OpenProjectBtn_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            foreach (var projectType in ProjectTypesContainer.Items)
            {
                picker.FileTypeFilter.Add(projectType.Extension);
            }

            var files = await picker.PickMultipleFilesAsync();

            foreach (var file in files)
            {
                ViewModel.OpenProject(file);
            }
        }

        private async void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsFileStored == false)
            {
                var name = ViewModel.GetCurrentFileName();
                var extension = ViewModel.GetCurrentExtension();
                var file = await PickSaveFile(extension, name);

                await ViewModel.SaveProjectAsAsync(file);

                return;
            }

            await ViewModel.SaveProjectAsync();
        }

        private async void SaveAllBtn_Click(object sender, RoutedEventArgs e)
        {
            var tabs = ViewModel.GetModifiedTabs();

            foreach (var tab in tabs)
            {
                if (tab.File.IsStored == false)
                {
                    var file = await PickSaveFile(tab.File.Extension, tab.File.Name);
                    await tab.SaveAsAsync(file);
                }
                else
                {
                    await tab.SaveAsync();
                }
            }
        }

        private async Task<StorageFile> PickSaveFile(string extension, string suggestedName)
        {
            var picker = new FileSavePicker()
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            picker.FileTypeChoices.Add(extension, new List<string>() { extension });

            picker.SuggestedFileName = suggestedName;

            return await picker.PickSaveFileAsync();
        }
    }
}
