using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.Panelbar.Animation
{
    internal class TimelineDrawer
    {
        private double _lowerBoundary;
        private int _secondLengthInPixels;
        private double _marginLeft;
        private Color _textColor;
        private Color _ticksColor;
        private double _smallTickHeight;
        private double _middleTickHeight;
        private double _largeTickHeight;
        private Thickness _timeLabelMargin;
        private CanvasTextFormat _labelFormat;

        public TimelineDrawer(int secondLengthInPixels)
        {
            _secondLengthInPixels = secondLengthInPixels;
            _labelFormat = new CanvasTextFormat();

            InitStyles();
        }

        private void InitStyles()
        {
            var resources = Application.Current.Resources;

            _marginLeft = (double)resources["TimelineContainerPaddingLeft"];
            _ticksColor = (resources["TimelineTickFill"] as SolidColorBrush)?.Color ?? Colors.Transparent;
            _textColor = (resources["TimelineTextForeground"] as SolidColorBrush)?.Color ?? Colors.Transparent;
            _lowerBoundary = (double)resources["AnimationControlHeaderHeight"];

            _smallTickHeight = (double)resources["TimelineSmallTickHeight"];
            _middleTickHeight = (double)resources["TimelineMiddleTickHeight"];
            _largeTickHeight = (double)resources["TimelineLargeTickHeight"];

            _timeLabelMargin = (Thickness)resources["TimelineTimeLabelMargin"];

            _labelFormat.FontSize = 12;
        }

        public void SetSecondLength(int secondLengthInPixels)
        {
            _secondLengthInPixels = secondLengthInPixels;
        }

        public void Draw(CanvasDrawingSession ds, Rect region)
        {
            if (region.Top > _lowerBoundary)
                return;

            ds.Antialiasing = CanvasAntialiasing.Aliased;

            DrawTimelineLabels(ds, region);

            DrawTicks(ds, region, (float)_largeTickHeight, 1, _ticksColor);
            DrawTicks(ds, region, (float)_middleTickHeight, 2, _ticksColor);
            DrawTicks(ds, region, (float)_smallTickHeight, 10, _ticksColor);
        }

        private void DrawTimelineLabels(CanvasDrawingSession ds, Rect region)
        {
            int startX = (int)(Math.Floor(region.Left / _secondLengthInPixels) * _secondLengthInPixels);
            int endX = (int)(Math.Ceiling(region.Right / _secondLengthInPixels) * _secondLengthInPixels);

            for (int x = startX; x <= endX; x += _secondLengthInPixels)
            {
                var time = TimeSpan.FromSeconds(x / _secondLengthInPixels);

                var labelX = x + (float)_timeLabelMargin.Left + (float)_marginLeft;
                var labelY = (float)_lowerBoundary - (float)_timeLabelMargin.Bottom;

                ds.DrawText(time.ToString("c"), labelX, labelY, _textColor, _labelFormat);
            }
        }

        private void DrawTicks(CanvasDrawingSession ds, Rect region, float height, int lengthDivider, Color color)
        {
            var dividedInterval = _secondLengthInPixels / lengthDivider;

            int startX = (int)(Math.Floor(region.Left / dividedInterval) * dividedInterval);
            int endX = (int)(Math.Ceiling(region.Right / dividedInterval) * dividedInterval);

            for (int x = startX; x <= endX; x += dividedInterval)
            {
                var tickX = x + (float)_marginLeft;

                ds.DrawLine(tickX, (float)_lowerBoundary, tickX, (float)_lowerBoundary - height, color);
            }
        }
    }

}
