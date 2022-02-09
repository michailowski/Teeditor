using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Teeditor.Common.Models.Bindable
{
    public abstract class BindableBase : INotifyPropertyChanged, INotifyPropertyModificated
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<PropertyModificatedEventArgs> PropertyModificated;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void OnPropertyModificated(string propertyName, string fieldName, object oldValue, object newValue)
            => PropertyModificated?.Invoke(this, new PropertyModificatedEventArgs(this, propertyName, fieldName, oldValue, newValue));

        protected bool Set<T>(ref T storage, T value, string storageName = null, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            var oldValue = storage;
            var newValue = value;

            storage = value;

            OnPropertyChanged(propertyName);

            if (string.IsNullOrEmpty(storageName) == false)
            {
                OnPropertyModificated(propertyName, storageName, oldValue, newValue);
            }

            return true;
        }
    }
}