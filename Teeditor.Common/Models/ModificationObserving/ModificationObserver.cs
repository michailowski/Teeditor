using Teeditor.Common.Enumerations;
using Teeditor.Common.Models.Commands;
using Teeditor.TeeWorlds.MapExtension.Internal.Commands;

namespace Teeditor.Common.Models.ModificationObserving
{
    public class ModificationObserver : ModificationObserverBase, IModificationObserver
    {
        private bool _isModified;
        private CommandManager _commandManager;

        public bool IsModified
        {
            get => _isModified;
            set => Set(ref _isModified, value);
        }

        public ModificationObserver(CommandManager commandManager)
        {
            _commandManager = commandManager;

            Modified += ModificationObserver_Modified;
        }

        private void ModificationObserver_Modified(object sender, ModificationEventArgs e)
        {
            IsModified = true;

            if (e.Source == ModificationSource.Property)
            {
                var propertyModificationCommand = new PropertyModificationCommand(e.Carrier, e.PropertyName, e.FieldName, e.OldValue, e.NewValue);

                propertyModificationCommand.Execute(_commandManager);
            }
        }
    }
}
