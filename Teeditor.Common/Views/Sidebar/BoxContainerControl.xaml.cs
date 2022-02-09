using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Common.Views.Sidebar
{
    public sealed partial class BoxContainerControl : UserControl
    {
        internal UIElement Header => HeaderBorder;
        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string),
                typeof(BoxContainerControl), new PropertyMetadata(null));

        public UIElement BoxContent
        {
            get => (UIElement)GetValue(BoxContentProperty);
            set => SetValue(BoxContentProperty, value);
        }

        public static readonly DependencyProperty BoxContentProperty =
            DependencyProperty.Register("Content", typeof(UIElement),
                typeof(BoxContainerControl), new PropertyMetadata(null, ContentProperty_ChangedCallback));

        public IList<MenuFlyoutItemBase> MenuItems
        {
            get => (IList<MenuFlyoutItemBase>)GetValue(MenuItemsProperty);
            set => SetValue(MenuItemsProperty, value);
        }

        public static readonly DependencyProperty MenuItemsProperty =
            DependencyProperty.Register("Content", typeof(IList<MenuFlyoutItemBase>),
                typeof(BoxContainerControl), new PropertyMetadata(null, MenuItemsProperty_ChangedCallback));

        internal event EventHandler DockToggleNeeded;
        internal event EventHandler MoveUpNeeded;
        internal event EventHandler MoveDownNeeded;
        internal event EventHandler CloseNeeded;

        internal event EventHandler<BoxControl> DropToUpNeeded;
        internal event EventHandler<BoxControl> DropToDownNeeded;

        public BoxContainerControl()
        {
            this.InitializeComponent();

            AllowDrop = true;

            DragEnter += BoxContainerControl_DragEnter;
            DragLeave += BoxContainerControl_DragLeave;
            DragOver += BoxContainerControl_DragOver;
        }

        private static void MenuItemsProperty_ChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BoxContainerControl)d;
            var items = (IList<MenuFlyoutItemBase>)e.NewValue;

            control.Menu.Items.Clear();

            foreach (var item in items)
            {
                control.Menu.Items.Add(item);
            }

            if (items.Any())
            {
                control.Menu.Items.Add(new MenuFlyoutSeparator());
            }

            foreach (var item in control.MainMenuFlyout.Items)
            {
                control.Menu.Items.Add(item);
            }
        }

        private static void ContentProperty_ChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BoxContainerControl)d;

            control.ContentContainer.Children.Clear();
            control.ContentContainer.Children.Add((UIElement)e.NewValue);
        }

        private void MoveUpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MoveUpNeeded?.Invoke(this, EventArgs.Empty);
        }

        private void ChangeDockMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DockToggleNeeded?.Invoke(this, EventArgs.Empty);
        }

        private void MoveDownMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MoveDownNeeded?.Invoke(this, EventArgs.Empty);
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            CloseNeeded?.Invoke(this, EventArgs.Empty);
        }

        private void DropToUp_Drop(object sender, DragEventArgs e)
        {
            DropOverlay.Visibility = Visibility.Collapsed;

            e.DataView.Properties.TryGetValue("DraggedBox", out var box);

            if (box == null)
                return;

            DropToUpNeeded?.Invoke(this, (BoxControl)box);
        }

        private void DropToDown_Drop(object sender, DragEventArgs e)
        {
            DropOverlay.Visibility = Visibility.Collapsed;

            e.DataView.Properties.TryGetValue("DraggedBox", out var box);

            if (box == null)
                return;

            DropToDownNeeded?.Invoke(this, (BoxControl)box);
        }

        private void DropTo_DragOver(object sender, DragEventArgs e)
        {
            e.DataView.Properties.TryGetValue("DraggedBoxContainer", out var boxContainer);

            if (boxContainer == null)
                return;

            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
        }

        private void DropTo_DragEnter(object sender, DragEventArgs e)
        {
            var btn = (UIElement)sender;

            btn.Opacity = 0.6;
        }

        private void DropTo_DragLeave(object sender, DragEventArgs e)
        {
            var btn = (UIElement)sender;

            btn.Opacity = 1;
        }

        private void BoxContainerControl_DragOver(object sender, DragEventArgs e)
        {
            e.DataView.Properties.TryGetValue("DraggedBoxContainer", out var boxContainer);

            if (boxContainer == null)
                return;

            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;
        }

        private void BoxContainerControl_DragEnter(object sender, DragEventArgs e)
        {
            e.DataView.Properties.TryGetValue("DraggedBoxContainer", out var boxContainer);

            if (boxContainer == null || boxContainer == this)
                return;

            DropOverlay.Visibility = Visibility.Visible;
        }

        private void BoxContainerControl_DragLeave(object sender, DragEventArgs e)
        {
            DropOverlay.Visibility = Visibility.Collapsed;
        }
    }
}