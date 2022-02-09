using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.Storage;
using Windows.UI.Core;
using Teeditor.ViewModels;
using System;
using Teeditor.Common.Views.Sidebar;
using Teeditor.Common.Models.Sidebar;

namespace Teeditor.Views
{
    internal sealed partial class SidebarControl : UserControl
    {
        public SidebarViewModel Source
        {
            get => (SidebarViewModel)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(SidebarViewModel),
                typeof(SidebarControl), new PropertyMetadata(null, SourceProperty_ChangedCallback));
        
        public SidebarDock Dock
        {
            get => (SidebarDock)GetValue(DockProperty);
            set => SetValue(DockProperty, value);
        }

        public static readonly DependencyProperty DockProperty =
            DependencyProperty.Register("Dock", typeof(SidebarDock),
                typeof(SidebarControl), new PropertyMetadata(null, DockProperty_ChangedCallback));

        public SidebarControl()
        {
            this.InitializeComponent();
        }

        #region Main Logic

        private void Source_TabUpdated(object sender, EventArgs e)
        {
            InitItems();
            ResetVisibility();
            ResetSplitters();
        }

        private static void DockProperty_ChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SidebarControl)d;

            if (control.Dock == SidebarDock.Left)
            {
                Grid.SetColumn(control.ItemsGrid, 0);
                Grid.SetColumn(control.VerticalSplitter, 1);
                Grid.SetColumn(control.DropPlace, 2);
            }
            else if (control.Dock == SidebarDock.Right)
            {
                Grid.SetColumn(control.ItemsGrid, 2);
                Grid.SetColumn(control.VerticalSplitter, 1);
                Grid.SetColumn(control.DropPlace, 0);
            }
        }

        #endregion

        #region Items Logic

        private void InitItems()
        {
            ItemsGrid.Children.Clear();

            var items = Source.GetItemsByDock(Dock);

            if (items == null)
                return;

            for (int i = 0; i < items.Count; i++)
            {
                items[i].SetValue(Grid.RowProperty, i);

                ItemsGrid.Children.Add(items[i]);
            }
        }

        private void ResetItems()
        {
            var items = Source.GetItemsByDock(Dock);

            if (items == null)
                return;

            for (int i = 0; i < items.Count; i++)
            {
                items[i].SetValue(Grid.RowProperty, i);
            }
        }

        private void ResetSplitters()
        {
            ItemsGrid.RowDefinitions.Clear();

            var gridSplitters = ItemsGrid.Children.Where(x => x.GetType() == typeof(GridSplitter));

            while (gridSplitters.Any())
            {
                ItemsGrid.Children.Remove(gridSplitters.FirstOrDefault());
            }

            var count = Source.GetActiveItemsCountByDock(Dock);

            for (int i = 0; i < count; i++)
            {
                ItemsGrid.RowDefinitions.Add(
                    new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

                if (i > 0)
                {
                    var style = (Style)Application.Current.Resources["SidebarGridSplitterHorizontalStyle"];

                    var gridSplitter = new GridSplitter()
                    {
                        Style = style
                    };

                    gridSplitter.SetValue(Grid.RowProperty, i);

                    ItemsGrid.Children.Add(gridSplitter);
                }
            }
        }

        private void ResetVisibility()
        {
            var activeCount = Source.GetActiveItemsCountByDock(Dock);

            if (activeCount == 0)
            {
                ItemsGrid.Width = double.NaN;
                VerticalSplitter.Visibility = Visibility.Collapsed;
            }
            else
            {
                var localWidth = ApplicationData.Current.LocalSettings.Values[$"Sidebar{Dock}Width"];

                if (localWidth != null)
                    ItemsGrid.Width = (double) localWidth;

                VerticalSplitter.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Source Event Handlers

        private static void SourceProperty_ChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
                return;

            var control = (SidebarControl)d;

            var oldViewModel = (SidebarViewModel)e.OldValue;

            if (oldViewModel != null)
            {
                oldViewModel.TabUpdated -= control.Source_TabUpdated;
                oldViewModel.BoxDockChanged -= control.Source_BoxDockChanged;
                oldViewModel.BoxOrderChanged -= control.Source_BoxOrderChanged;
                oldViewModel.BoxVisibilityChanged -= control.Source_BoxVisibilityChanged;
                oldViewModel.BoxDragStarted -= control.Source_BoxDragStarted;
                oldViewModel.BoxDragEnded -= control.Source_BoxDragEnded;
            }

            if (e.NewValue == null)
                return;

            var newViewModel = (SidebarViewModel)e.NewValue;

            newViewModel.TabUpdated += control.Source_TabUpdated;
            newViewModel.BoxDockChanged += control.Source_BoxDockChanged;
            newViewModel.BoxOrderChanged += control.Source_BoxOrderChanged;
            newViewModel.BoxVisibilityChanged += control.Source_BoxVisibilityChanged;
            newViewModel.BoxDragStarted += control.Source_BoxDragStarted;
            newViewModel.BoxDragEnded += control.Source_BoxDragEnded;
        }

        private void Source_BoxDockChanged(object sender, SidebarItemChangedEventArgs e)
        {
            if (e.Box.ViewModel.Dock != Dock)
            {
                ItemsGrid.Children.Remove(e.Box);
            }
            else
            {
                ItemsGrid.Children.Add(e.Box);
            }

            ResetItems();
            ResetVisibility();
            ResetSplitters();
        }

        private void Source_BoxOrderChanged(object sender, SidebarItemChangedEventArgs e)
        {
            if (e.Box.ViewModel.Dock != Dock)
                return;

            ResetItems();
            ResetVisibility();
        }

        private void Source_BoxVisibilityChanged(object sender, SidebarItemChangedEventArgs e)
        {
            if (e.Box.ViewModel.Dock != Dock)
                return;

            ResetItems();
            ResetVisibility();
            ResetSplitters();
        }

        private void Source_BoxDragStarted(object sender, SidebarItemChangedEventArgs e)
        {
            var count = Source.GetActiveItemsCountByDock(Dock);

            if (count > 0)
                return;

            DropPlace.Visibility = Visibility.Visible;
        }

        private void Source_BoxDragEnded(object sender, SidebarItemChangedEventArgs e)
        {
            DropPlace.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region DropPlace Event Handlers

        private void DropPlace_Drop(object sender, DragEventArgs e)
        {
            e.DataView.Properties.TryGetValue("DraggedBox", out var propertyValue);

            if (propertyValue == null)
                return;

            var box = (BoxControl)propertyValue;

            if (box.ViewModel.Dock == Dock)
                return;

            box.ToggleDock();
        }

        private void DropPlace_DragEnter(object sender, DragEventArgs e)
        {
            e.DataView.Properties.TryGetValue("DraggedBoxContainer", out var boxContainer);

            if (boxContainer == null)
                return;

            var dropPlaceRect = (UIElement)sender;

            dropPlaceRect.Opacity = 0.6;
        }

        private void DropPlace_DragLeave(object sender, DragEventArgs e)
        {
            var dropPlaceRect = (UIElement)sender;

            dropPlaceRect.Opacity = 1;
        }

        private void DropPlace_DragOver(object sender, DragEventArgs e)
        {
            e.DataView.Properties.TryGetValue("DraggedBoxContainer", out var boxContainer);

            if (boxContainer == null)
                return;

            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;
        }

        #endregion

        #region VerticalSplitter Event Handlers

        private void VerticalSplitter_OnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeWestEast, 1);
        }

        private void VerticalSplitter_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (Dock == SidebarDock.Left)
                ItemsGrid.Width = ItemsGrid.ActualWidth + e.Delta.Translation.X;
            else if (Dock == SidebarDock.Right)
                ItemsGrid.Width = ItemsGrid.ActualWidth - e.Delta.Translation.X;
        }

        private void VerticalSplitter_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values[$"Sidebar{Dock}Width"] = ItemsGrid.Width;

            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
        }

        private void VerticalSplitter_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (Window.Current.CoreWindow.PointerCursor.Id == 0)
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
        }

        private void VerticalSplitter_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (Window.Current.CoreWindow.PointerCursor.Id == 0)
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeWestEast, 0);
        }

        #endregion
    }
}