using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Linq;
using Teeditor.Common.Helpers;
using Teeditor.Common.Views.Panelbar;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.Panelbar.Animation;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Panelbar;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Panelbar.Animation
{
    internal sealed partial class AnimationPanelControl : PanelControl
    {
        private new AnimationPanelViewModel ViewModel => (AnimationPanelViewModel)_viewModel;

        private DispatcherTimer _boosterScrollTimer;
        private int _boosterScrollSpeed = 3;
        private TimeSpan _boosterScrollTimerInterval;
        private DateTimeOffset _boosterScrollStartTime;
        private double _fixedContentHorizontalOffset;

        public AnimationPanelControl(AnimationPanelViewModel viewModel)
            : base(viewModel)
        {
            this.InitializeComponent();

            viewModel.ForceContentRegionsInvalidation += ViewModel_ForceRegionInvalidation;

            _boosterScrollTimer = new DispatcherTimer();
            _boosterScrollTimerInterval = new TimeSpan(0, 0, 0, 0, 1);
            _boosterScrollTimer.Interval = _boosterScrollTimerInterval;
            _boosterScrollTimer.Tick += BoosterScrollTimer_Tick;
        }

        private void ViewModel_ForceRegionInvalidation(object sender, EventArgs e)
            => ContentCanvas.Invalidate();

        private void ContentCanvas_RegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
        {
            var mergedInvalidatedRegions = Rect.Empty;

            foreach (var region in args.InvalidatedRegions)
            {
                mergedInvalidatedRegions.Union(region);
            }

            var forcedInvalidatedRegion = ViewModel.GetInvalidatedRegions(mergedInvalidatedRegions);

            if (forcedInvalidatedRegion.IsEmpty == false)
            {
                using (var ds = sender.CreateDrawingSession(forcedInvalidatedRegion))
                {
                    ViewModel.DrawEnvelopes(ds, forcedInvalidatedRegion);
                }

                return;
            }

            foreach (var region in args.InvalidatedRegions)
            {
                using (var ds = sender.CreateDrawingSession(region))
                {
                    ViewModel.DrawEnvelopes(ds, region);
                }
            }
        }

        private void TimelineCanvas_RegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
        {
            foreach (var region in args.InvalidatedRegions)
            {
                using (var ds = sender.CreateDrawingSession(region))
                {
                    ViewModel.DrawTimeline(ds, region);
                }
            }
        }

        private void ContentScroll_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            TimelineScroll.ChangeView(ContentScroll.HorizontalOffset, null, null, true);
            TimeCursorScroll.ChangeView(ContentScroll.HorizontalOffset, null, null, true);
            TimeCursorLineScroll.ChangeView(ContentScroll.HorizontalOffset, null, null, true);

            if (e.IsIntermediate == false)
                return;

            EnvelopesListScroll.ChangeView(null, ContentScroll.VerticalOffset, null, true);
        }

        #region Content Pointer Event Handlers

        private void ContentCanvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var currentPoint = e.GetCurrentPoint(ContentScroll);
            var newHorizontalOffset = ContentScroll.HorizontalOffset - currentPoint.Properties.MouseWheelDelta;

            ContentScroll.ChangeView(newHorizontalOffset, null, null);

            ViewModel.TryContentMoveByOffset(-currentPoint.Properties.MouseWheelDelta);

            e.Handled = true;
        }

        private void ContentCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(ContentCanvas);
            var globalPointerPosition = Window.Current.CoreWindow.PointerPosition;

            ViewModel.SetContentPointerPosition(pointer.Position);
            ViewModel.TryContentDragging(globalPointerPosition);
        }

        private void ContentCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint((UIElement)sender);

            if (pointer.Properties.IsLeftButtonPressed)
            {
                var globalPointerPosition = Window.Current.CoreWindow.PointerPosition;

                var draggingStarted = ViewModel.TryStartContentDragging(globalPointerPosition);

                if (draggingStarted)
                {
                    ActivateBoosters();
                }
            }
            else if (pointer.Properties.IsRightButtonPressed)
            {
                TryShowKeyframeContextMenu(pointer);
                TryShowSegmentContextMenu(pointer);
                ViewModel.ResetPointerPosition();
            }
        }

        private void ContentCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint((UIElement)sender);

            if (pointer.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
            {
                ViewModel.EndContentDragging();

                DeactivateBoosters();
            }
        }

        private void ContentCanvas_PointerExited(object sender, PointerRoutedEventArgs e)
        { 
            ViewModel.ResetPointerPosition();
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
        }

        #endregion

        #region Booster's Logic

        private void ActivateBoosters()
        {
            RightKeyframeBoosterGrid.IsHitTestVisible = true;

            if (ContentScroll.HorizontalOffset == 0)
                return;

            LeftKeyframeBoosterGrid.IsHitTestVisible = true;
        }
        
        private void DeactivateBoosters()
            => LeftKeyframeBoosterGrid.IsHitTestVisible = RightKeyframeBoosterGrid.IsHitTestVisible = false;

        private void LeftKeyframeBoosterGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            LeftKeyframeBoosterGrid.Opacity = 1;
            ViewModel.TryStartContentDragBoosting(KeyframeBoostingDirection.Left);
            StartBoosterScroll();
        }

        private void LeftKeyframeBoosterGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            LeftKeyframeBoosterGrid.Opacity = 0;
            ViewModel.EndContentDragBoosting();
            StopBoosterScroll();
        }

        private void RightKeyframeBoosterGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            RightKeyframeBoosterGrid.Opacity = 1;
            ViewModel.TryStartContentDragBoosting(KeyframeBoostingDirection.Right);
            StartBoosterScroll();
        }

        private void RightKeyframeBoosterGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            RightKeyframeBoosterGrid.Opacity = 0;
            ViewModel.EndContentDragBoosting();
            StopBoosterScroll();
        }

        private void StartBoosterScroll()
        {
            _boosterScrollStartTime = DateTimeOffset.Now;
            _fixedContentHorizontalOffset = ContentScroll.HorizontalOffset;
            _boosterScrollTimer.Start();
        }

        private void StopBoosterScroll()
        {
            _boosterScrollTimer.Stop();
            _fixedContentHorizontalOffset = 0;
        }
        
        private void BoosterScrollTimer_Tick(object sender, object e)
        {
            var time = _boosterScrollStartTime - DateTimeOffset.Now;
            var pixels = ViewModel.GetPixelsByTime(time * _boosterScrollSpeed);

            if (LeftKeyframeBoosterGrid.Opacity > 0)
            {
                ContentScroll.ChangeView(_fixedContentHorizontalOffset + pixels, null, null, true);
            }
            else if (RightKeyframeBoosterGrid.Opacity > 0)
            {
                ContentScroll.ChangeView(_fixedContentHorizontalOffset - pixels, null, null, true);
            }
        }

        #endregion

        private void TryShowKeyframeContextMenu(PointerPoint pointer)
        {
            if (ViewModel.HoveredKeyframe.IsEmpty)
                return;

            Flyout contextFlyout = ViewModel.HoveredKeyframe.Point is MapEnvelopePointColor ? ColorKeyframeFlyout : PositionKeyframeFlyout;

            var flyoutContent = (StackPanel)contextFlyout.Content;
            flyoutContent.DataContext = ViewModel.HoveredKeyframe.Point;

            var timeBoxBorder = (Border)flyoutContent.Children[1];
            var timeBox = timeBoxBorder.Child as TimeSpanControl;

            timeBox.MinValue = ViewModel.HoveredKeyframe.MinTime;
            timeBox.MaxValue = ViewModel.HoveredKeyframe.MaxTime;

            var flyoutOptions = new FlyoutShowOptions();
            flyoutOptions.Position = pointer.Position;
            flyoutOptions.Placement = FlyoutPlacementMode.Top;

            contextFlyout.ShowAt(ContentCanvas, flyoutOptions);
        }

        private void TryShowSegmentContextMenu(PointerPoint pointer)
        {
            if (ViewModel.HoveredSegment.IsEmpty)
                return;

            var flyoutOptions = new FlyoutShowOptions();
            flyoutOptions.Position = pointer.Position;
            flyoutOptions.ShowMode = FlyoutShowMode.Standard;

            EnvelopeSectionView.Source = ViewModel.HoveredSegment;
            CurveTypeSelectFlyout.ShowAt(ContentCanvas, flyoutOptions);
        }

        private async void ColorPickerBtn_Click(object sender, RoutedEventArgs e)
        {
            var eyedropper = new Eyedropper();
            var button = (Button)sender;
            var envPoint = (MapEnvelopePointColor)button.DataContext;

            envPoint.Color = await eyedropper.Open();
        }

        private void TimeCursorScroll_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (e.KeyModifiers != Windows.System.VirtualKeyModifiers.Control)
                return;

            var intervalChanged = false;

            if (e.GetCurrentPoint((UIElement)sender).Properties.MouseWheelDelta > 0)
            {
                intervalChanged = ViewModel.TryIncreaseTimelineInterval();
            }
            else
            {
                intervalChanged = ViewModel.TryDecreaseTimelineInterval();
            }

            if (intervalChanged)
            {
                TimelineCanvas.Invalidate();
                ContentCanvas.Invalidate();
            }
        }

        private void TimeCursorGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var grid = (Grid) sender;
                var pointer = e.GetCurrentPoint(grid);

                if (pointer.Properties.IsLeftButtonPressed)
                {
                    var pixels = pointer.Position.X - grid.Padding.Left - TimeCursor.ActualWidth / 2;

                    ViewModel.TrySetTimeByPixels(pixels);
                }
            }
        }

        private void TimeResetBtn_Click(object sender, RoutedEventArgs e)
            => ViewModel.ResetTimer();

        private void TimeCursor_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var cursor = (Button)sender;

            var pixels = cursor.Margin.Left + e.Delta.Translation.X;

            ViewModel.TrySetTimeByPixels(pixels);
        }

        private void AddColorEnvelopeMenuItem_Click(object sender, RoutedEventArgs e)
        { 
            ViewModel?.AddColorEnvelope();

            EnvelopesListScroll.UpdateLayout();

            EnvelopesListScroll.ChangeView(null, EnvelopesListScroll.ScrollableHeight, null, false);
            ContentScroll.ChangeView(null, ContentScroll.ScrollableHeight, null, false);
        }

        private void AddPositionEnvelopeMenuItem_Click(object sender, RoutedEventArgs e)
        { 
            ViewModel?.AddPositionEnvelope();

            EnvelopesListScroll.UpdateLayout();

            EnvelopesListScroll.ChangeView(null, EnvelopesListScroll.ScrollableHeight, null, false);
            ContentScroll.ChangeView(null, ContentScroll.ScrollableHeight, null, false);
        }

        private void EnvelopesListScroll_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (e.IsIntermediate == false)
                return;

            ContentScroll.ChangeView(null, EnvelopesListScroll.VerticalOffset, null, true);
        }

        private void AddKeyframeBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var envelope = (MapEnvelope)button.DataContext;

            ViewModel.AddKeyframeAtTimeCursor(envelope);
        }

        private void AddKeyframeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var button = (MenuFlyoutItem)sender;
            var envelope = (MapEnvelope)button.DataContext;

            ViewModel.AddKeyframeAtTimeCursor(envelope);
        }

        private void EnvelopePropertiesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var button = (MenuFlyoutItem)sender;

            ViewModel.OpenProperties((MapEnvelope)button.DataContext);
        }

        private void RemoveEnvelopeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuFlyoutItem)sender;
            var envelope = menuItem.DataContext as MapEnvelope;

            ViewModel.RemoveEnvelopeWithBindings(envelope);
        }

        private void RemoveKeyframeBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var point = btn.DataContext as MapEnvelopePoint;

            ViewModel.RemoveEnvelopePoint(point);

            ColorKeyframeFlyout.Hide();
            PositionKeyframeFlyout.Hide();
        }

        private void SidebarEnvelopeHeader_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Mouse)
                return;

            var pointer = e.GetCurrentPoint(SidebarItems);

            if (pointer.Properties.IsRightButtonPressed)
            {
                var syncItem = SidebarEnvelopeMenuFlyout.Items.FirstOrDefault(x => x.Name == "EnvelopeSyncMenuItem");
                var container = e.OriginalSource as FrameworkElement;

                var binding = new Binding
                {
                    Source = container.DataContext,
                    Path = new PropertyPath("IsSynchronized"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                BindingOperations.SetBinding(syncItem, ToggleMenuFlyoutItem.IsCheckedProperty, binding);

                SidebarEnvelopeMenuFlyout.ShowAt((Grid)sender);
            }
        }

        private void SidebarEnvelopeHeader_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var btnsGrid = VisualHierarchyHelper.FindChild<Grid>(grid, "BtnsGrid");

            double width = 0;

            for (int i = 0; i < btnsGrid.Children.Count; i++)
            {
                width += (btnsGrid.Children[i] as FrameworkElement).Width;
            }

            var storyboard = new Storyboard();
            var animation = new DoubleAnimation()
            {
                EnableDependentAnimation = true,
                From = 0,
                To = width,
                Duration = new Duration(TimeSpan.FromMilliseconds(100))
            };

            Storyboard.SetTargetName(animation, btnsGrid.Name);
            Storyboard.SetTarget(animation, btnsGrid);
            Storyboard.SetTargetProperty(animation, "Width");

            storyboard.Children.Add(animation);
            storyboard.Begin();

            grid.Background = (Brush)Application.Current.Resources["EnvelopeHeaderPointerOverBackground"];
        }

        private void SidebarEnvelopeHeader_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var btnsGrid = VisualHierarchyHelper.FindChild<Grid>(grid, "BtnsGrid");

            var storyboard = new Storyboard();
            var animation = new DoubleAnimation()
            {
                EnableDependentAnimation = true,
                From = btnsGrid.Width,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(100))
            };

            Storyboard.SetTargetName(animation, btnsGrid.Name);
            Storyboard.SetTarget(animation, btnsGrid);
            Storyboard.SetTargetProperty(animation, "Width");

            storyboard.Children.Add(animation);
            storyboard.Begin();

            grid.Background = (Brush)Application.Current.Resources["EnvelopeHeaderBackground"];
        }
    }
}
