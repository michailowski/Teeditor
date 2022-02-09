
using System;

namespace Teeditor.Common.Models.ModificationObserving
{
    public abstract class ModificationObservingStrategyBase : ModificationObserverBase
    {
        protected IModificationObservable _observableModel;
        private bool _isStarted;

        public void SetObservableModel(IModificationObservable observableModel)
        {
            _observableModel = observableModel;
            _observableModel.Modified += ObservableModel_Modified;
        }

        public void StartObserving()
        {
            if (_isStarted == true)
                return;

            Initialize();

            _isStarted = true;
        }

        private void ObservableModel_Modified(object sender, EventArgs e)
            => RaiseModification(sender);

        protected virtual void Initialize()
        {
            // Do nothing
        }
    }
}