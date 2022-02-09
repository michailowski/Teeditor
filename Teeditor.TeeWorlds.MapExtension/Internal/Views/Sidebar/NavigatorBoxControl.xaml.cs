using System;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Input;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls.Primitives;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Teeditor.Common.Views.Sidebar;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Sidebar
{
    internal sealed partial class NavigatorBoxControl : BoxControl
    {
        private bool _isMouseCaptured;

        private new NavigatorBoxViewModel ViewModel => (NavigatorBoxViewModel)_viewModel;

        public NavigatorBoxControl(NavigatorBoxViewModel viewModel)
            : base(viewModel)
        {
            this.InitializeComponent();

            viewModel.MinimapUpdated += ViewModel_MinimapUpdated;
            viewModel.ViewboxUpdated += ViewModel_ViewboxUpdated;
            viewModel.ZoomLevelChanged += ViewModel_ZoomLevelChanged;
        }

        protected override void DoAfterDockChanging()
            => VirtualCanvas.Invalidate();
        
        private void ViewModel_ZoomLevelChanged(object sender, float value)
            => ZoomSlider.Value = value;
        
        private void ViewModel_ViewboxUpdated(object sender, EventArgs e)
        {
            ViewBox.Width = ViewModel.ViewBox.Width;
            ViewBox.Height = ViewModel.ViewBox.Height;

            Canvas.SetLeft(ViewBox, ViewModel.ViewBox.X);
            Canvas.SetTop(ViewBox, ViewModel.ViewBox.Y);
        }

        private void ViewModel_MinimapUpdated(object sender, EventArgs e)
        {
            ViewModel.CalcParameters((float)VirtualCanvas.Width, (float)VirtualCanvas.Height);
            VirtualCanvas.Invalidate();
        }

        private void VirtualCanvas_RegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
        {
            using (var cds = sender.CreateDrawingSession(args.VisibleRegion))
            {
                ViewModel.DrawChecksBackground(cds, (float)sender.Width, (float)sender.Height);
                ViewModel.DrawMinimap(cds);
            }
        }

        private void VirtualCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            VirtualCanvas.Invalidate();
        }

        private void ViewBox_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var pointer = e.GetCurrentPoint((Rectangle)sender);

                if (pointer.Properties.IsLeftButtonPressed)
                {
                    ViewModel.ViewBoxStartDrag(Window.Current.CoreWindow.PointerPosition.ToVector2());
                    _isMouseCaptured = true;

                    Window.Current.CoreWindow.PointerMoved += CoreWindow_PointerMoved;
                    Window.Current.CoreWindow.PointerReleased += CoreWindow_PointerReleased;
                }
            }
        }

        private void CoreWindow_PointerMoved(CoreWindow sender, PointerEventArgs args)
        {
            var properties = args.CurrentPoint.Properties;

            if (properties.IsLeftButtonPressed)
            {
                if (_isMouseCaptured)
                {
                    if (args.CurrentPoint.Position.X < Window.Current.Bounds.X + Window.Current.Bounds.Width)
                    {
                        ViewModel.ViewBoxBeingDrag(Window.Current.CoreWindow.PointerPosition.ToVector2());
                    }
                }

                args.Handled = true;
            }
        }

        private void CoreWindow_PointerReleased(CoreWindow sender, PointerEventArgs args)
        {
            Window.Current.CoreWindow.PointerMoved -= CoreWindow_PointerMoved;
            Window.Current.CoreWindow.PointerReleased -= CoreWindow_PointerReleased;

            if (args.CurrentPoint.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
            {
                _isMouseCaptured = false;

                ViewModel.ViewBoxEndDrag();
            }
        }

        private void ZoomOutBtn_Click(object sender, RoutedEventArgs e) 
            => ViewModel.TryDecreaseZoomLevel();

        private void ZoomInBtn_Click(object sender, RoutedEventArgs e) 
            => ViewModel.TryIncreaseZoomLevel();

        private void ZoomSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) 
            => ViewModel.SetZoomLevel((float)e.NewValue);
    }
}
