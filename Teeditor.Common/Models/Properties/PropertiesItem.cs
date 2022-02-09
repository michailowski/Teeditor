using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Utilities;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Common.Models.Properties
{
    public class PropertiesItem<T> : BindableBase, IPropertiesItem where T : struct
    {
        private T _value;

        public T Value
        {
            get => _value;
            set => Set(ref _value, value);
        }
        public string MenuText { get; set; }
        public PathIcon MenuIcon { get; }

        public PropertiesItem(T value, string menuText, string menuIconResourceKey)
        {
            Value = value;
            MenuText = menuText;
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources[menuIconResourceKey]) };
        }
    }
}
