using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Teeditor.Common.ViewModels
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class AffectsOtherPropertyAttribute : Attribute
    {
        public AffectsOtherPropertyAttribute(string otherPropertyName)
            => this.AffectsProperty = otherPropertyName;
        
        public string AffectsProperty { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PropertyProviderAttribute : Attribute
    {
        public PropertyProviderAttribute(params string[] propertyNames)
            => this.PropertyNames = propertyNames;

        public string[] PropertyNames { get; private set; }
    }

    // https://docs.microsoft.com/en-us/archive/msdn-magazine/2010/july/design-patterns-problems-and-solutions-with-model-view-viewmodel
    public abstract class DynamicViewModel : DynamicObject, INotifyPropertyChanged
    {
        private object _dynamicModel;
        private bool _modelRaisesPropertyChangedEvents;
        private List<INotifyPropertyChanged> _notifyPropertyCarriers = new List<INotifyPropertyChanged>();

        public event PropertyChangedEventHandler PropertyChanged;

        public object DynamicModel 
        { 
            get => _dynamicModel;
            set => SetDynamicModel(value);
        }

        protected async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }

        protected bool Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void SetDynamicModel(object model)
        {
            UnsubscribeNotifyPropertyCarriers();

            _dynamicModel = model;

            UpdateNotifyPropertyCarriers();

            SubscribeNotifyPropertyCarriers();
        }

        private void SubscribeNotifyPropertyCarriers()
        {
            foreach (var carrier in _notifyPropertyCarriers)
            {
                carrier.PropertyChanged += Carrier_PropertyChanged;
            }

            this._modelRaisesPropertyChangedEvents = _notifyPropertyCarriers.Any();
        }

        private void Carrier_PropertyChanged(object sender, PropertyChangedEventArgs e)
            => this.OnPropertyChanged(e.PropertyName);
        

        private void UnsubscribeNotifyPropertyCarriers()
        {
            foreach (var carrier in _notifyPropertyCarriers)
            {
                carrier.PropertyChanged -= Carrier_PropertyChanged;
            }

            this._modelRaisesPropertyChangedEvents = false;
        }

        private void UpdateNotifyPropertyCarriers()
        {
            _notifyPropertyCarriers.Clear();

            var viewModelProperties = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var propertyInfo in viewModelProperties)
            {
                var provider = propertyInfo.GetCustomAttribute<PropertyProviderAttribute>();

                var propertyCarrier = FindPropertyCarrier(provider, this.DynamicModel);

                if (propertyCarrier is INotifyPropertyChanged notifiedPropertyCarrier)
                {
                    var property = propertyCarrier?.GetType().GetProperty(propertyInfo.Name);

                    if (property == null || property.CanRead == false || _notifyPropertyCarriers.Contains(notifiedPropertyCarrier))
                        continue;

                    _notifyPropertyCarriers.Add(notifiedPropertyCarrier);
                }
            }
        }

        private object FindPropertyCarrier(PropertyProviderAttribute provider, object parent)
        {
            if (parent == null)
                return null;

            object findedPropertyCarrier = parent;

            if (provider == null)
                return findedPropertyCarrier;

            foreach (var name in provider.PropertyNames)
            {
                var propertyInfo = findedPropertyCarrier.GetType().GetProperty(name);

                if (propertyInfo == null || propertyInfo.CanRead == false)
                    return null;

                findedPropertyCarrier = propertyInfo.GetValue(findedPropertyCarrier, null);
            }

            return findedPropertyCarrier;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var viewModelProperty = this.GetType().GetProperty(binder.Name);
            var provider = viewModelProperty.GetCustomAttribute<PropertyProviderAttribute>();

            var propertyCarrier = FindPropertyCarrier(provider, this.DynamicModel);
            var property = propertyCarrier?.GetType().GetProperty(binder.Name);

            if (property == null || property.CanRead == false)
            {
                result = null;
                return false;
            }

            result = property.GetValue(propertyCarrier, null);
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var viewModelProperty = this.GetType().GetProperty(binder.Name);
            var provider = viewModelProperty.GetCustomAttribute<PropertyProviderAttribute>();

            var propertyCarrier = FindPropertyCarrier(provider, this.DynamicModel);
            var property = propertyCarrier?.GetType().GetProperty(binder.Name);

            if (property == null || property.CanRead == false)
                return false;

            property.SetValue(propertyCarrier, value, null);

            if (this._modelRaisesPropertyChangedEvents == false)
                this.OnPropertyChanged(property.Name);

            var affectsProps = property.GetCustomAttributes(typeof(AffectsOtherPropertyAttribute), true);

            foreach (AffectsOtherPropertyAttribute otherPropertyAttr in affectsProps)
            {
                this.OnPropertyChanged(otherPropertyAttr.AffectsProperty);
            }

            return true;
        }
    }
}
