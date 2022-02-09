using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Teeditor.Common.Models.Tab;
using Teeditor.ViewModels;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Teeditor.Views
{
    internal sealed partial class TabsControl : UserControl
    {
        public TabsViewModel Source
        {
            get => (TabsViewModel)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(TabsViewModel),
                typeof(ToolbarControl), new PropertyMetadata(null));

        public TabsControl()
        {
            this.InitializeComponent();
        }

        private async void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            var tab = (ITab)args.Tab.DataContext;

            if (tab.ModificationObserver.IsModified)
            {
                ContentDialog dialog = new ContentDialog();

                dialog.Title = "Save changes to the following item?";
                dialog.PrimaryButtonText = "Save";
                dialog.SecondaryButtonText = "Don't Save";
                dialog.CloseButtonText = "Cancel";
                dialog.DefaultButton = ContentDialogButton.Primary;

                var projectBorder = new Border();
                projectBorder.Padding = new Thickness(8, 4, 8, 4);
                projectBorder.Background = new SolidColorBrush((Color)Application.Current.Resources["SystemBaseLowColor"]);

                var projectName = new TextBlock();
                projectName.Text = tab.File.Name + tab.File.Extension;
                projectBorder.Child = projectName;

                dialog.Content = projectBorder;

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
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
                else if (result == ContentDialogResult.None)
                {
                    return;
                }
            }

            Source?.CloseTab(tab);
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
