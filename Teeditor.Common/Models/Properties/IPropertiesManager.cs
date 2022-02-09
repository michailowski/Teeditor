using System.Runtime.CompilerServices;

namespace Teeditor.Common.Models.Properties
{
    internal interface IPropertiesManager
    {
        bool TryAddProperty<U>(string name, PropertiesItem<U> property) where U : struct;
        PropertiesItem<U> GetProperty<U>([CallerMemberName] string name = null) where U : struct;
        bool TrySetPropertyValue<U>(U newValue, [CallerMemberName] string name = null) where U : struct;
    }
}
