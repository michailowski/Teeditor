using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Controls;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Teeditor.Common.Utilities;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.Panelbar.Animation;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Panelbar.Animation
{
    internal sealed partial class EnvelopeSectionControl : UserControl
    {
        private readonly Color[] _lineColors = new Color[4] { Colors.DarkRed, Colors.DarkGreen, Colors.DarkBlue, Colors.LightYellow };
        private readonly string[] _lineNamesForColor = new string[4] { "Red", "Green", "Blue", "Alpha" };
        private readonly string[] _lineNamesForPosition = new string[3] { "X", "Y", "Rotate" };
        private Path _draggablePointPath;
        private Point _lastPointPathPosition;
        private Point _pointerPositionOnClick;

        private ObservableCollection<EnvelopeSegmentChanel> Channels { get; set; }
        private List<CurveTypeContainer> CurveTypeContainers { get; set; }

        public EnvelopeSegment Source
        {
            get => (EnvelopeSegment)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(EnvelopeSegment),
                typeof(EnvelopeSectionControl), new PropertyMetadata(null, new PropertyChangedCallback(Source_Changed)));

        private static void Source_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (EnvelopeSectionControl)d;

            control.FillChannels();
        }

        public EnvelopeSectionControl()
        {
            this.InitializeComponent();

            Channels = new ObservableCollection<EnvelopeSegmentChanel>();
            CurveTypeContainers = new List<CurveTypeContainer>();

            FillCurveTypeContainers();
        }

        private void FillChannels()
        {
            Channels.Clear();

            for (int i = 0; i < Source.Envelope.ChannelsNumber; i++)
            {
                var chanelTang = new EnvelopeSegmentChanel(
                    i,
                    Source.LeftPoint,
                    Source.RightPoint,
                    200,
                    Source.Envelope.Type == EnvelopeType.Color ? _lineNamesForColor[i] : _lineNamesForPosition[i],
                    _lineColors[i]);

                Channels.Add(chanelTang);
            }
        }

        private void FillCurveTypeContainers()
        {
            var linear = new CurveTypeContainer("Linear", (int)CurveType.Linear, true, false, (string)Resources["LinearCurvePath"]);
            var stepEnd = new CurveTypeContainer("Step End", (int)CurveType.Step, true, false, (string)Resources["StepEndCurvePath"]);
            var easeInCubic = new CurveTypeContainer("Ease In Cubic", (int)CurveType.Slow, true, false, (string)Resources["EaseInCubicCurvePath"]);
            var easeOutCubic = new CurveTypeContainer("Ease Out Cubic", (int)CurveType.Fast, true, false, (string)Resources["EaseOutCubicCurvePath"]);
            var easeInOutCubic = new CurveTypeContainer("Ease In Out Cubic", (int)CurveType.Smooth, true, false, (string)Resources["EaseInOutCubicCurvePath"]);
            var customBezier = new CurveTypeContainer("Custom Bezier", (int)CurveType.Bezier, true, true, startPoint: new Point(90, 90), endPoint: new Point(110, 110));

            CurveTypeContainers.Add(linear);
            CurveTypeContainers.Add(stepEnd);
            CurveTypeContainers.Add(easeInCubic);
            CurveTypeContainers.Add(easeOutCubic);
            CurveTypeContainers.Add(easeInOutCubic);
            CurveTypeContainers.Add(customBezier);
        }

        private UIElement GetGraphPath(int curveTypeId)
        {
            var container = CurveTypeContainers.Find(x => x.CurveTypeId == curveTypeId);

            if (container == null)
                return UserInterface.PathMarkupToGraphPath((string)Resources["EmptyCurvePath"]);

            return container.PathData == null ?
                UserInterface.PathMarkupToGraphPath((string)Resources["EmptyCurvePath"]) :
                UserInterface.PathMarkupToGraphPath(container.PathData);
        }

        private Visibility GetVisibilityForEditable(int curveTypeId)
        {
            var container = CurveTypeContainers.Find(x => x.CurveTypeId == curveTypeId);

            if (container == null)
                return Visibility.Collapsed;

            return container.IsEditable ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility GetVisibilityForNonEditable(int curveTypeId)
        {
            var container = CurveTypeContainers.Find(x => x.CurveTypeId == curveTypeId);

            if (container == null)
                return Visibility.Collapsed;

            return container.IsEditable ? Visibility.Collapsed : Visibility.Visible;
        }

        private void TangentPoint_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var pointer = e.GetCurrentPoint(EnvelopeViewBorder);

                if (pointer.Properties.IsLeftButtonPressed)
                {
                    var pointPath = (Path)sender;
                    var ellipse = (EllipseGeometry)pointPath.Data;

                    _lastPointPathPosition = ellipse.Center;
                    _draggablePointPath = pointPath;

                    _pointerPositionOnClick = pointer.Position;
                }
            }
        }

        private void TangentPoint_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Mouse) 
                return;

            var pointer = e.GetCurrentPoint(EnvelopeViewBorder);

            if (pointer.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonReleased) 
                return;

            _pointerPositionOnClick = new Point(0, 0);
            _lastPointPathPosition = new Point(0, 0);
            _draggablePointPath = null;
        }

        private void EnvelopeViewBorder_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                var pointer = e.GetCurrentPoint((Border)sender);

                if (pointer.Properties.IsLeftButtonPressed)
                {
                    if (_draggablePointPath != null)
                    {
                        UpdateDraggablePointPath(pointer);
                    }
                }
            }

            e.Handled = true;
        }

        private void UpdateDraggablePointPath(PointerPoint pointer)
        {
            var delta = new Point(pointer.Position.X - _pointerPositionOnClick.X, pointer.Position.Y - _pointerPositionOnClick.Y);
            var newPointPathPosition = new Point(_lastPointPathPosition.X + delta.X, _lastPointPathPosition.Y + delta.Y);

            var chanel = (EnvelopeSegmentChanel)_draggablePointPath.DataContext;

            if (chanel == null)
                return;

            if ((string)_draggablePointPath.Tag == "Start")
                chanel.StartTangent = newPointPathPosition;
            else if ((string)_draggablePointPath.Tag == "End")
                chanel.EndTangent = newPointPathPosition;
        }

        private void CurrentCurveComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var container = (CurveTypeContainer)e.AddedItems[0];
            Source.LeftPoint.CurveTypeId = container.CurveTypeId;
        }

        private void MainGrid_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Mouse) 
                return;

            var pointer = e.GetCurrentPoint((Grid)sender);

            if (!pointer.Properties.IsRightButtonPressed) 
                return;

            ShowGraphContextMenu(pointer);

            e.Handled = true;
        }

        private void ShowGraphContextMenu(PointerPoint pointer)
        {
            var container = CurveTypeContainers.Find(x => x.CurveTypeId == Source.LeftPoint.CurveTypeId);

            if (container == null || !container.IsEditable)
                return;

            var menu = new MenuFlyout();

            var isAllVisible = true;

            foreach (var chanel in Channels)
                if (!chanel.IsVisible)
                    isAllVisible = false;


            var allBtn = new RadioMenuFlyoutItem
            {
                GroupName = "EnvelopeVisibility",
                Text = "Show All",
                IsChecked = isAllVisible
            };

            allBtn.Click += (s, e) => {
                foreach (var chanel in Channels)
                    chanel.IsVisible = true;
            };

            menu.Items?.Add(allBtn);
            menu.Items?.Add(new MenuFlyoutSeparator());

            foreach (var chanel in Channels)
            {
                var chanelBtn = new RadioMenuFlyoutItem
                {
                    GroupName = "EnvelopeVisibility",
                    Text = $"Show {chanel.Name}",
                    IsChecked = !isAllVisible && chanel.IsVisible
                };

                chanelBtn.Click += (s, e) =>
                {
                    foreach (var chanelItem in Channels)
                        if (chanelItem != chanel)
                            chanelItem.IsVisible = false;

                    chanel.IsVisible = true;
                };

                menu.Items?.Add(chanelBtn);
            }

            menu.ShowAt(MainGrid, pointer.Position);
        }
    }
}
