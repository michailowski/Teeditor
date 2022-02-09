using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Panelbar.Animation
{
    internal sealed partial class TimeSpanControl : UserControl
    {
        private TimeSpan _minValue = TimeSpan.Zero;
        private TimeSpan _maxValue = TimeSpan.MaxValue;

        public TimeSpan Value
        {
            get => (TimeSpan)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(TimeSpan),
                typeof(TimeSpanControl), new PropertyMetadata(null, new PropertyChangedCallback(Value_Changed)));

        public TimeSpan MinValue
        {
            get => (TimeSpan)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(TimeSpan),
                typeof(TimeSpanControl), new PropertyMetadata(null, new PropertyChangedCallback(MinValue_Changed)));

        public TimeSpan MaxValue
        {
            get => (TimeSpan)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(TimeSpan),
                typeof(TimeSpanControl), new PropertyMetadata(null, new PropertyChangedCallback(MaxValue_Changed)));

        public TimeSpanControl()
        {
            InitializeComponent();
        }

        private static void Value_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TimeSpanControl)d;

            control.Value = (TimeSpan)e.NewValue;
        }
        private static void MinValue_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TimeSpanControl)d;

            control._minValue = (TimeSpan)e.NewValue;
        }
        private static void MaxValue_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TimeSpanControl)d;

            control._maxValue = (TimeSpan)e.NewValue;
        }

        public TimeSpan Clamp(TimeSpan value, TimeSpan min, TimeSpan max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }

        private void HoursBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            var newValue = new TimeSpan(0, (int)args.NewValue, Value.Minutes, Value.Seconds, Value.Milliseconds);

            Value = Clamp(newValue, _minValue, _maxValue);
        }

        private void SecondsBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            var newValue = new TimeSpan(0, Value.Hours, Value.Minutes, (int)args.NewValue, Value.Milliseconds);

            Value = Clamp(newValue, _minValue, _maxValue);
        }

        private void MinutesBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            var newValue = new TimeSpan(0, Value.Hours, (int)args.NewValue, Value.Seconds, Value.Milliseconds);

            Value = Clamp(newValue, _minValue, _maxValue);
        }

        private void MillisecondsBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            var newValue = new TimeSpan(0, Value.Hours, Value.Minutes, Value.Seconds, (int)args.NewValue);

            Value = Clamp(newValue, _minValue, _maxValue);
        }
    }
}
