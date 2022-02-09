using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Teeditor.Common.Helpers
{
    public static class DataConverterHelper
    {
        public static double BoolToDouble(bool value, string trueResult, string falseResult) 
            => value ? Double.Parse(trueResult) : Double.Parse(falseResult);

        public static bool InversedBool(bool value) => !value;

        public static Visibility BoolToVisibility(bool value) 
            => value ? Visibility.Visible : Visibility.Collapsed;

        public static Visibility BoolToVisibilityInverted(bool value)
            => value ? Visibility.Collapsed : Visibility.Visible;

        public static Visibility VisibileIfEqual(object value1, object value2)
            => value1.Equals(value2) ? Visibility.Visible : Visibility.Collapsed;

        public static SolidColorBrush ColorToBrush(Color color) => new SolidColorBrush(color);
    }
}
