using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Teeditor.Common.Views.Toolbar
{
    public sealed partial class ToolContainerControl : UserControl
    {
        internal UIElement Divider => DividerBorder;

        public UIElement ToolContent
        {
            get => (UIElement)GetValue(ToolContentProperty);
            set => SetValue(ToolContentProperty, value);
        }

        public static readonly DependencyProperty ToolContentProperty =
            DependencyProperty.Register("Content", typeof(UIElement),
                typeof(ToolContainerControl), new PropertyMetadata(null, ContentProperty_ChangedCallback));

        internal event EventHandler<ToolControl> DropToLeftNeeded;
        internal event EventHandler<ToolControl> DropToRightNeeded;

        public ToolContainerControl()
        {
            this.InitializeComponent();

            this.AllowDrop = true;

            this.DragEnter += ToolContainerControl_DragEnter;
            this.DragLeave += ToolContainerControl_DragLeave;
            this.DragOver += ToolContainerControl_DragOver;
        }

        private static void ContentProperty_ChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ToolContainerControl)d;

            control.ContentContainer.Children.Clear();
            control.ContentContainer.Children.Add((UIElement) e.NewValue);
        }

        private void ToolContainerControl_DragOver(object sender, DragEventArgs e)
        {
            e.DataView.Properties.TryGetValue("DraggedToolContainer", out var toolContainer);

            if (toolContainer == null)
                return;

            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;
        }

        private void ShowDropPlaces()
        {
            DropPlaceLeft.Visibility = Visibility.Visible;
            DropPlaceRight.Visibility = Visibility.Visible;
        }

        private void HideDropPlaces()
        {
            DropPlaceLeft.Visibility = Visibility.Collapsed;
            DropPlaceRight.Visibility = Visibility.Collapsed;
        }

        private void ToolContainerControl_DragEnter(object sender, DragEventArgs e)
        {
            e.DataView.Properties.TryGetValue("DraggedToolContainer", out var toolContainer);

            if (toolContainer == null || toolContainer == this)
                return;

            ShowDropPlaces();
        }

        private void ToolContainerControl_DragLeave(object sender, DragEventArgs e)
            => HideDropPlaces();

        private void DividerBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeAll, 0);
        }

        private void DividerBorder_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
        }

        private void DropPlaceLeft_Drop(object sender, DragEventArgs e)
        {
            HideDropPlaces();

            e.DataView.Properties.TryGetValue("DraggedTool", out var tool);

            if (tool == null)
                return;

            DropToLeftNeeded?.Invoke(this, (ToolControl)tool);
        }

        private void DropPlaceRight_Drop(object sender, DragEventArgs e)
        {
            HideDropPlaces();

            e.DataView.Properties.TryGetValue("DraggedTool", out var tool);

            if (tool == null)
                return;

            DropToRightNeeded?.Invoke(this, (ToolControl)tool);
        }

        private void DropPlace_DragOver(object sender, DragEventArgs e)
        {
            e.DataView.Properties.TryGetValue("DraggedToolContainer", out var toolContainer);

            if (toolContainer == null)
                return;

            e.AcceptedOperation = DataPackageOperation.Move;
        }

        private void DropPlace_DragEnter(object sender, DragEventArgs e)
        {
            var btn = (UIElement)sender;

            btn.Opacity = 0.6;
        }

        private void DropPlace_DragLeave(object sender, DragEventArgs e)
        {
            var btn = (UIElement)sender;

            btn.Opacity = 1;
        }
    }
}