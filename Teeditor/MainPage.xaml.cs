using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teeditor.Models;
using Teeditor.ViewModels;
using Teeditor.ViewModels.Toolbar;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Teeditor
{
    internal sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            InitializeViewModels();
            InitializeTitlebarButtonsStyles();


            Window.Current.SetTitleBar(TitleBar);
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += MainPage_CloseRequested;

#if DEBUG
            CanvasDevice.DebugLevel = CanvasDebugLevel.Information;
#endif
        }

        private void InitializeViewModels()
        {
            var tabsContainer = new TabsContainer();

            DataContext = ViewModel = new MainViewModel(tabsContainer);
            Editor.Source = new EditorViewModel();
            Toolbar.Source = new ToolbarViewModel(tabsContainer);
            Panelbar.Source = new PanelbarViewModel();
            SidebarLeft.Source = new SidebarViewModel();
            SidebarRight.Source = new SidebarViewModel();
            MainMenu.Source = new MainMenuViewModel(tabsContainer);
            Tabs.Source = new TabsViewModel(tabsContainer);
            Tabs.Source.PropertyChanged += TabsSource_PropertyChanged;
        }

        private void InitializeTitlebarButtonsStyles()
        {
            ApplicationView appView = ApplicationView.GetForCurrentView();

            appView.TitleBar.BackgroundColor = Colors.Transparent;
            appView.TitleBar.ButtonBackgroundColor = (Color)Application.Current.Resources["SystemChromeBlackMediumLowColor"];
            appView.TitleBar.ButtonForegroundColor = Colors.DarkGray;
            appView.TitleBar.ButtonInactiveBackgroundColor = appView.TitleBar.ButtonBackgroundColor;
            appView.TitleBar.ButtonInactiveForegroundColor = Colors.DarkSlateGray;
        }

        private void TabsSource_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "SelectedTab")
                return;

            var tab = Tabs.Source.SelectedTab;

            Editor.Source.SetTab(tab);
            SidebarLeft.Source.SetTab(tab);
            SidebarRight.Source.SetTab(tab);
            MainMenu.Source.SetTab(tab);
            Toolbar.Source.SetTab(tab);
            Panelbar.Source.SetTab(tab);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var args = e.Parameter as IActivatedEventArgs;

            if (args != null)
            {
                if (args.Kind == ActivationKind.File)
                {
                    var fileArgs = args as FileActivatedEventArgs;
                    var file = (StorageFile) fileArgs.Files[0];

                    ViewModel.OpenProject(file);
                }
            }
        }

        private async void MainPage_CloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            var modifiedTabs = ViewModel.GetModifiedTabs();

            if (modifiedTabs.Any() == false)
                return;

            e.Handled = true;

            ContentDialog dialog = new ContentDialog();

            dialog.Title = "Save changes to the following items?";
            dialog.PrimaryButtonText = "Save";
            dialog.SecondaryButtonText = "Don't Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;

            var stackPanel = new StackPanel();

            foreach (var tab in modifiedTabs)
            {
                var projectBorder = new Border();
                projectBorder.Padding = new Thickness(8, 4, 8, 4);
                projectBorder.Margin = new Thickness(0, 0, 0, 1);
                projectBorder.Background = new SolidColorBrush((Color)Application.Current.Resources["SystemBaseLowColor"]);

                var projectName = new TextBlock();
                projectName.Text = tab.File.Name;
                projectBorder.Child = projectName;

                stackPanel.Children.Add(projectBorder);
            }

            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                foreach (var tab in modifiedTabs)
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

                Application.Current.Exit();
            }
            else if (result == ContentDialogResult.Secondary)
            {
                Application.Current.Exit();
            }

            e.Handled = true;
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