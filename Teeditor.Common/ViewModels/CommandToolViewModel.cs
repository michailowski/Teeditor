using Teeditor.Common.Models.Commands;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Utilities;
using Teeditor.Common.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.ViewModels
{
    internal class CommandToolViewModel : ToolViewModelBase
    {
        private CommandManager _commandManager;

        public bool IsUndoAllowed => _commandManager.IsUndoAllowed;
        public bool IsRedoAllowed => _commandManager.IsRedoAllowed;

        public CommandToolViewModel()
        {
            Label = "Command Tool";
            MenuText = "Command Tool";
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["CommandToolIconPath"]) };
        }

        public override void SetTab(ITab tab)
        {
            _commandManager = tab?.CommandManager;

            // DynamicViewModelBase need this setting for using of binding to model with dynamic properties
            DynamicModel = _commandManager;
        }

        public void Undo() => _commandManager?.Undo();

        public void Redo() => _commandManager?.Redo();
    }
}
