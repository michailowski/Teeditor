using System;
using System.Collections.ObjectModel;
using Teeditor.Common.Models.Bindable;

namespace Teeditor.Common.Models.Commands
{
    public class CommandManager : BindableBase
    {
        private ObservableCollection<IUndoRedoableCommand> _commands;
        private int _currentCommandIndex = -1;
        private bool _isRedoAllowed = false;
        private bool _isUndoAllowed = false;
        private IUndoRedoableCommand _lastExecutedCommand;

        public bool IsUndoAllowed
        {
            get => _isUndoAllowed;
            private set => Set(ref _isUndoAllowed, value);
        }
        public bool IsRedoAllowed
        {
            get => _isRedoAllowed;
            private set => Set(ref _isRedoAllowed, value);
        }

        public ReadOnlyObservableCollection<IUndoRedoableCommand> Commands { get; }

        public CommandManager()
        {
            _commands = new ObservableCollection<IUndoRedoableCommand>();
            Commands = new ReadOnlyObservableCollection<IUndoRedoableCommand>(_commands);
        }

        public void AddCommand(IUndoRedoableCommand command)
        {
            RemoveAllAfterLastExecutedCommand();

            _commands.Add(command);

            IsUndoAllowed = true;
            IsRedoAllowed = false;

            _currentCommandIndex = _commands.Count - 1;
            RemarkLastExecutedCommand();
        }

        public void Redo(int levels = 1)
        {
            if (IsRedoAllowed == false)
                return;

            levels = Math.Min(levels, _commands.Count - 1 - _currentCommandIndex);

            for (int i = 0; i < levels; i++)
            {
                _currentCommandIndex++;
                _commands[_currentCommandIndex].Redo();
                _commands[_currentCommandIndex].IsExecuted = true;
            }

            RemarkLastExecutedCommand();

            IsUndoAllowed = levels > 0;
            IsRedoAllowed = _currentCommandIndex < _commands.Count - 1;
        }

        public void Undo(int levels = 1)
        {
            if (IsUndoAllowed == false)
                return;

            levels = Math.Min(levels, _currentCommandIndex + 1);

            for (int i = 0; i < levels; i++)
            {
                _commands[_currentCommandIndex].Undo();
                _commands[_currentCommandIndex].IsExecuted = false;
                _currentCommandIndex--;
            }

            RemarkLastExecutedCommand();

            IsUndoAllowed = _currentCommandIndex >= 0;
            IsRedoAllowed = levels > 0;
        }

        public void GoTo(IUndoRedoableCommand command)
        {
            var index = _commands.IndexOf(command);

            if (index > _currentCommandIndex)
            {
                Redo(index - _currentCommandIndex);
            }
            else if (index < _currentCommandIndex)
            {
                Undo(_currentCommandIndex - index);
            }
        }

        private void RemoveAllAfterLastExecutedCommand()
        {
            while (_commands.Count - 1 > _currentCommandIndex)
            {
                _commands.RemoveAt(_commands.Count - 1);
            }
        }

        private void RemarkLastExecutedCommand()
        {
            if (_lastExecutedCommand != null)
            {
                _lastExecutedCommand.IsLastExecuted = false;
            }

            if (_currentCommandIndex >= 0 && _currentCommandIndex < _commands.Count)
            {
                _commands[_currentCommandIndex].IsLastExecuted = true;
                _lastExecutedCommand = _commands[_currentCommandIndex];
            }
        }
    }
}
