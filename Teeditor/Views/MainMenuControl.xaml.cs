using System.Collections.Generic;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Teeditor.Converters;
using Teeditor.ViewModels;
using System;
using System.Linq;
using Windows.Storage.Pickers;
using Teeditor.Views.Dialogs;
using Teeditor.ViewModels.Dialogs;
using Teeditor.Models;
using Windows.Storage;
using System.Threading.Tasks;

namespace Teeditor.Views
{
    internal sealed partial class MainMenuControl : UserControl
    {
        public MainMenuViewModel Source
        {
            get => (MainMenuViewModel)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(MainMenuViewModel),
                typeof(MainMenuControl), new PropertyMetadata(null, SourceProperty_Changed));

        public MainMenuControl()
        {
            this.InitializeComponent();

            PrepareViewTab();
        }

        private static void SourceProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
                return;

            var control = (MainMenuControl)d;

            var oldViewModel = (MainMenuViewModel)e.OldValue;

            if (oldViewModel != null)
            {
                oldViewModel.TabUpdated -= control.ViewModel_TabUpdated;
            }

            if (e.NewValue == null)
                return;

            var newViewModel = (MainMenuViewModel)e.NewValue;
            newViewModel.TabUpdated += control.ViewModel_TabUpdated;
        }

        private void ViewModel_TabUpdated(object sender, EventArgs e)
        {
            PrepareViewTab();
        }

        private void PrepareViewTab()
        {
            while (ViewTab.Items.Count > 1)
            {
                ViewTab.Items.RemoveAt(0);
            }

            var menuFlyoutItems = new List<MenuFlyoutItemBase>();

            if (TryGetMenuSubItem(out var sidebarSubItem, "Sidebar", GetSidebarMenuItems()))
            {
                menuFlyoutItems.Add(sidebarSubItem);
            }

            if (TryGetMenuSubItem(out var toolbarSubItem, "Toolbar", GetToolbarMenuItems()))
            {
                menuFlyoutItems.Add(toolbarSubItem);
            }

            if (Source != null)
            {
                var propertiesItems = GetPropertiesMenuItems();

                if (menuFlyoutItems.Count > 0 && propertiesItems.Count() > 0)
                {
                    menuFlyoutItems.Add(new MenuFlyoutSeparator());
                }

                foreach (var item in propertiesItems)
                {
                    menuFlyoutItems.Add(item);
                }
            }

            if (menuFlyoutItems.Count > 0)
            {
                menuFlyoutItems.Add(new MenuFlyoutSeparator());
            }

            foreach (var item in menuFlyoutItems.Reverse<MenuFlyoutItemBase>())
            {
                ViewTab.Items.Insert(0, item);
            }
        }

        private bool TryGetMenuSubItem(out MenuFlyoutSubItem menuSubItem, string menuSubItemLabel, IEnumerable<MenuFlyoutItemBase> menuSubItemItems)
        {
            menuSubItem = null;

            if (Source == null || menuSubItemItems.Any() == false)
                return false;

            menuSubItem = new MenuFlyoutSubItem()
            {
                Text = menuSubItemLabel
            };

            foreach (var item in menuSubItemItems)
            {
                menuSubItem.Items.Add(item);
            }

            return true;
        }

        private IEnumerable<MenuFlyoutItemBase> GetSidebarMenuItems()
        {
            if (Source?.Boxes == null)
                yield break;

            foreach (var item in Source.Boxes)
            {
                var menuItem = new ToggleMenuFlyoutItem
                {
                    Text = item.ViewModel.MenuText,
                    Icon = item.ViewModel.MenuIcon
                };

                var binding = new Binding
                {
                    Source = item,
                    Path = new PropertyPath("Visibility"),
                    Converter = new VisibilityToBooleanConverter(),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                BindingOperations.SetBinding(menuItem, ToggleMenuFlyoutItem.IsCheckedProperty, binding);

                yield return menuItem;
            }
        }

        private IEnumerable<MenuFlyoutItemBase> GetToolbarMenuItems()
        {
            if (Source?.Tools == null)
                yield break;

            foreach (var item in Source.Tools)
            {
                var menuItem = new ToggleMenuFlyoutItem
                {
                    Text = item.ViewModel.MenuText,
                    Icon = item.ViewModel.MenuIcon
                };

                var binding = new Binding
                {
                    Source = item,
                    Path = new PropertyPath("Visibility"),
                    Converter = new VisibilityToBooleanConverter(),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                BindingOperations.SetBinding(menuItem, ToggleMenuFlyoutItem.IsCheckedProperty, binding);

                yield return menuItem;
            }
        }

        private IEnumerable<MenuFlyoutItemBase> GetPropertiesMenuItems()
        {
            if (Source?.Properties == null)
                yield break;

            foreach (var item in Source.Properties)
            {
                var property = item.Value;

                var toggleBtn = new ToggleMenuFlyoutItem
                {
                    Text = property.MenuText,
                    Icon = property.MenuIcon
                };

                var binding = new Binding
                {
                    Source = property,
                    Path = new PropertyPath("Value"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                BindingOperations.SetBinding(toggleBtn, ToggleMenuFlyoutItem.IsCheckedProperty, binding);

                yield return toggleBtn;
            }
        }

        private void ToggleFullScreenBtn_Click(object sender, RoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();

            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
            }
            else
            {
                view.TryEnterFullScreenMode();
            }
        }

        private void UndoMenuItem_Click(object sender, RoutedEventArgs e)
            => Source?.Undo();
        
        private void RedoMenuItem_Click(object sender, RoutedEventArgs e)
            => Source?.Redo();

        private async void CreateProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ProjectCreateDialog(new ProjectCreateDialogViewModel());

            ContentDialogResult result = await dlg.ShowAsync();

            var projectType = dlg.ViewModel.SelectedItem;

            dlg.ViewModel.SelectedItem = null;

            if (result != ContentDialogResult.Primary || projectType == null)
                return;

            Source?.CreateProject(projectType);
        }

        private async void OpenProjectMenuItem_Click(object sender, RoutedEventArgs e)
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
                Source?.OpenProject(file);
            }
        }

        private async void SaveProjectMenuItem_Click(object sender, RoutedEventArgs e)
            => await Source?.SaveProjectAsync();

        private async void SaveProjectAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var name = Source?.GetCurrentFileName();
            var extension = Source?.GetCurrentExtension();
            var file = await PickSaveFile(extension, name);

            await Source?.SaveProjectAsAsync(file);
        }

        private async void SaveAllProjectsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var tabs = Source?.GetModifiedTabs();

            if (tabs == null)
                return;

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

        private async void ShortcutReferenceBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ShortcutReferenceDialog();
            await dlg.ShowAsync();
        }

        private async void AboutBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AboutDialog();
            await dlg.ShowAsync();
        }

        private async void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SettingsDialog();
            await dlg.ShowAsync();
        }
    }
}
