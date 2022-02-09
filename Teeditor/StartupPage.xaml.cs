using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.Storage.Pickers;
using Windows.Storage;
using Teeditor.ViewModels;
using System.Linq;
using Teeditor.Models;
using Teeditor.Views.Dialogs;
using Teeditor.ViewModels.Dialogs;
using Teeditor.Common.Models.IO;

namespace Teeditor
{
    partial class StartupPage : Page
    {
        internal Rect splashImageRect; // Rect to store splash screen image coordinates
        private SplashScreen splash; // Variable to hold the splash screen object
        internal bool dismissed = false; // Variable to track splash screen dismissal status
        internal Frame rootFrame;

        public StartupPage(SplashScreen splashscreen, bool loadState)
        {
            InitializeComponent();

            // Listen for window resize events to reposition the extended splash screen image accordingly.
            // This is important to ensure that the extended splash screen is formatted properly in response to snapping, unsnapping, rotation, etc...
            Window.Current.SizeChanged += new WindowSizeChangedEventHandler(StartupPage_OnResize);

            splash = splashscreen;

            if (splash != null)
            {
                // Register an event handler to be executed when the splash screen has been dismissed.
                splash.Dismissed += new TypedEventHandler<SplashScreen, Object>(DismissedEventHandler);

                // Retrieve the window coordinates of the splash screen image.
                splashImageRect = splash.ImageLocation;
                PositionImage();
            }

            // Create a Frame to act as the navigation context
            rootFrame = new Frame();

            // Restore the saved session state if necessary
            RestoreState(loadState);

            SetMostRecentlyListSource();
        }
        private async void SetMostRecentlyListSource()
        {
            MostRecentlyList.ItemsSource = await MostRecentlyUsedList.GetAsync();
        }

        void RestoreState(bool loadState)
        {
            if (loadState)
            {
                // TODO: write code to load state
            }
        }

        // Position the extended splash screen image in the same location as the system splash screen image.
        void PositionImage()
        {
            ExtendedSplashImage.SetValue(Canvas.LeftProperty, splashImageRect.X);
            ExtendedSplashImage.SetValue(Canvas.TopProperty, splashImageRect.Y);
            ExtendedSplashImage.Height = splashImageRect.Height;
            ExtendedSplashImage.Width = splashImageRect.Width;
        }

        void StartupPage_OnResize(Object sender, WindowSizeChangedEventArgs e)
        {
            // Safely update the extended splash screen image coordinates. This function will be fired in response to snapping, unsnapping, rotation, etc...
            if (splash != null)
            {
                // Update the coordinates of the splash screen image.
                splashImageRect = splash.ImageLocation;
                PositionImage();
            }
        }

        // Include code to be executed when the system has transitioned from the splash screen to the extended splash screen (application's first view).
        void DismissedEventHandler(SplashScreen sender, object e)
        {
            dismissed = true;

            // Complete app setup operations here...
        }

        private async void CreateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (rootFrame.Navigate(typeof(MainPage)) == false)
                return;
            
            var p = (MainPage) rootFrame.Content;
            var mvm = (MainViewModel) p.DataContext;

            var dlg = new ProjectCreateDialog(new ProjectCreateDialogViewModel());

            ContentDialogResult result = await dlg.ShowAsync();

            var projectType = dlg.ViewModel.SelectedItem;

            dlg.ViewModel.SelectedItem = null;

            if (result != ContentDialogResult.Primary || projectType == null)
                return;

            mvm.CreateProject(projectType);

            Window.Current.Content = rootFrame;
        }

        private async void OpenProjectButton_Click(object sender, RoutedEventArgs e)
        {
            if (rootFrame.Navigate(typeof(MainPage)) == false)
                return;

            var p = (MainPage) rootFrame.Content;
            var mvm = (MainViewModel) p.DataContext;

            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            foreach (var projectType in ProjectTypesContainer.Items)
            {
                picker.FileTypeFilter.Add(projectType.Extension);
            }

            var files = await picker.PickMultipleFilesAsync();

            foreach (StorageFile file in files)
            {
                mvm.OpenProject(file);
            }

            if (files.Any() == false)
                return;

            Window.Current.Content = rootFrame;
        }

        private void MostRecentlyList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (rootFrame.Navigate(typeof(MainPage)) == false)
                return;

            MainGrid.Children.Clear();

            var p = (MainPage) rootFrame.Content;
            var mvm = (MainViewModel) p.DataContext;
            var mrf = (MostRecentlyItem) e.ClickedItem;

            mvm.OpenProject(mrf.File);

            Window.Current.Content = rootFrame;
        }
    }
}
