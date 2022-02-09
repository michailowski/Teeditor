using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Teeditor.Converters
{
    public class VisibilityToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }

            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool visibility)
            {
                return visibility ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }
    }
}
