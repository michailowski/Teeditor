using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Teeditor.Common.Models.Properties
{
    public abstract class PropertiesManagerBase : IPropertiesManager
    {
        private Dictionary<string, IPropertiesItem> _properties = new Dictionary<string, IPropertiesItem>();

        public IReadOnlyDictionary<string, IPropertiesItem> Items => _properties;

        public delegate void PropertyValueHandler(string name);
        public event PropertyValueHandler PropertyValueUpdated;

        public bool TryAddProperty<U>(string name, PropertiesItem<U> property) where U : struct
        {
            var isAdded = _properties.TryAdd(name, property);

            if (isAdded)
            {
                property.PropertyChanged += delegate (object s, PropertyChangedEventArgs e)
                {
                    PropertyValueUpdated?.Invoke(name);
                };
            }

            return isAdded;
        }

        public PropertiesItem<U> GetProperty<U>([CallerMemberName] string name = null) where U : struct
        {
            var propertyExists = _properties.TryGetValue(name, out var propertiesItem);
            var property = propertiesItem as PropertiesItem<U>;

            if (!propertyExists || property == null)
            {
                return new PropertiesItem<U>(default, "Undefined Property", "DockChangeIconPath");
            }

            return property;
        }

        public bool TrySetPropertyValue<U>(U newValue, [CallerMemberName] string name = null) where U : struct
        {
            var propertyExists = _properties.TryGetValue(name, out var propertiesItem);
            var property = propertiesItem as PropertiesItem<U>;

            if (!propertyExists || property == null)
                return false;

            property.Value = newValue;
            return true;
        }
    }
}
