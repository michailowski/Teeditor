using System;

namespace Teeditor.Common.Models.Bindable
{
    public class PropertyModificatedEventArgs : EventArgs
    {
        public object Carrier { get; }
        public string PropertyName { get; }
        public string FieldName { get; }
        public object NewValue { get; }
        public object OldValue { get; }

        public PropertyModificatedEventArgs(object carrier, string propertyName, string fieldName, object oldValue, object newValue)
        {
            Carrier = carrier;
            PropertyName = propertyName;
            FieldName = fieldName;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
