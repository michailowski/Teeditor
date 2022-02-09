using System.ComponentModel;

namespace Teeditor.Common.Models.Commands
{
    public interface IUndoRedoableCommand : INotifyPropertyChanged
    {
        string Name { get; }
        bool IsExecuted { get; set; }
        bool IsLastExecuted { get; set; }
        void Execute(CommandManager commandManager);
        void Redo();
        void Undo();
    }
}
