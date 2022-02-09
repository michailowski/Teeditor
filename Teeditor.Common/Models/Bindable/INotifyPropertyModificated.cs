using System;

namespace Teeditor.Common.Models.Bindable
{
    public interface INotifyPropertyModificated
    {
        event EventHandler<PropertyModificatedEventArgs> PropertyModificated;
    }
}
