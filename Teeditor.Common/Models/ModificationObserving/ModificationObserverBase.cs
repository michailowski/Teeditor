using System;
using System.Collections.Generic;
using Teeditor.Common.Models.Bindable;

namespace Teeditor.Common.Models.ModificationObserving
{
    public class ModificationObserverBase : BindableBase
    {
        private Dictionary<IModificationObservable, ModificationObservingStrategyBase> _observingDictionary;

        protected event EventHandler<ModificationEventArgs> Modified;

        public ModificationObserverBase()
        {
            _observingDictionary = new Dictionary<IModificationObservable, ModificationObservingStrategyBase>();
        }

        public void Add(IModificationObservable observableModel, ModificationObservingStrategyBase observingStrategy)
        {
            observingStrategy.SetObservableModel(observableModel);
            observingStrategy.Modified += ObservingStrategy_Modified;
            observingStrategy.StartObserving();

            _observingDictionary.Add(observableModel, observingStrategy);
        }

        public void Remove(IModificationObservable observableModel)
            => _observingDictionary.Remove(observableModel);
        
        protected void RaiseModification(PropertyModificatedEventArgs propertyModificatedEventArgs) 
            => Modified?.Invoke(this, new ModificationEventArgs(propertyModificatedEventArgs));

        protected void RaiseModification(object carrier)
            => Modified?.Invoke(this, new ModificationEventArgs(carrier));

        private void ObservingStrategy_Modified(object sender, ModificationEventArgs e)
            => Modified?.Invoke(sender, e);
    }
}
