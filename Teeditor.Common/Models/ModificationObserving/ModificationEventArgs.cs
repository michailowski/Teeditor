using System;
using Teeditor.Common.Enumerations;
using Teeditor.Common.Models.Bindable;

namespace Teeditor.Common.Models.ModificationObserving
{
    public class ModificationEventArgs : EventArgs
    {
        public object Carrier { get; }
        public ModificationSource Source { get; }
        public string PropertyName { get; }
        public string FieldName { get; }
        public object NewValue { get; }
        public object OldValue { get; }

        public ModificationEventArgs(object carrier)
        {
            Carrier = carrier;
            Source = ModificationSource.Forced;
        }

        public ModificationEventArgs(PropertyModificatedEventArgs propertyModificatedEventArgs)
        {
            Carrier = propertyModificatedEventArgs.Carrier;
            PropertyName = propertyModificatedEventArgs.PropertyName;
            FieldName = propertyModificatedEventArgs.FieldName;
            NewValue = propertyModificatedEventArgs.NewValue;
            OldValue = propertyModificatedEventArgs.OldValue;
            Source = ModificationSource.Property;
        }
    }
}
