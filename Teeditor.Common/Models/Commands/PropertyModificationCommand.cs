using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Teeditor.Common.Models.Commands;
using Teeditor.Common.Models.Bindable;
using System.Reflection;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Commands
{
    internal class PropertyModificationCommand : BindableBase, IUndoRedoableCommand
    {
        private object _propertyCarrier;
        private string _propertyName;
        private string _fieldName;

        private object _oldValue;
        private object _newValue;

        private FieldInfo _fieldInfo;
        private MethodInfo _propertyChangedMethodInfo;

        private bool _isExecuted = true;
        private bool _isLastExecuted = true;

        private string _name;

        public string Name => string.IsNullOrEmpty(_name) ? $"Property \"{_propertyName}\" Value Changed" : _name;
        
        public bool IsExecuted
        {
            get => _isExecuted;
            set => Set(ref _isExecuted, value);
        }

        public bool IsLastExecuted
        {
            get => _isLastExecuted;
            set => Set(ref _isLastExecuted, value);
        }

        public PropertyModificationCommand(object propertyCarrier, string propertyName, string fieldName, object oldValue, object newValue)
        {
            _propertyCarrier = propertyCarrier;
            _propertyName = propertyName;
            _fieldName = fieldName;
            _oldValue = oldValue;
            _newValue = newValue;

            var carrierType = _propertyCarrier.GetType();

            while (carrierType != null)
            {
                _fieldInfo = carrierType.GetField(_fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

                if (_fieldInfo != null)
                    break;

                carrierType = carrierType.BaseType;
            }

            _propertyChangedMethodInfo = carrierType.GetMethod("OnPropertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);

            var propertyInfo = carrierType.GetProperty(_propertyName, BindingFlags.Instance | BindingFlags.Public);
            var labelAttribute = propertyInfo.GetCustomAttribute<ModificationCommandLabelAttribute>();

            if (labelAttribute == null)
                return;

            _name = labelAttribute.Label;
        }

        public void Execute(CommandManager commandManager)
        {
            var ignoredAction = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                commandManager.AddCommand(this);
            });
        }

        public void Redo()
        {
            if (_fieldInfo == null || _propertyChangedMethodInfo == null)
                return;

            _fieldInfo.SetValue(_propertyCarrier, _newValue);
            _propertyChangedMethodInfo.Invoke(_propertyCarrier, new object[] { _propertyName });
        }

        public void Undo()
        {
            if (_fieldInfo == null || _propertyChangedMethodInfo == null)
                return;

            _fieldInfo.SetValue(_propertyCarrier, _oldValue);
            _propertyChangedMethodInfo.Invoke(_propertyCarrier, new object[] { _propertyName });
        }
    }
}
