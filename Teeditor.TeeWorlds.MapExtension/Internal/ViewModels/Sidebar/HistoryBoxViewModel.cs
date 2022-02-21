using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Teeditor.Common.Models.Commands;
using Teeditor.Common.Models.Sidebar;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Utilities;
using Teeditor.Common.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar
{
    internal class HistoryBoxViewModel : BoxViewModelBase
    {
        private CommandManager _commandManager;
        private ReadOnlyObservableCollection<IUndoRedoableCommand> _commands;

        internal ReadOnlyObservableCollection<IUndoRedoableCommand> Commands
        {
            get => _commands;
            private set => Set(ref _commands, value);
        }

        internal event EventHandler<object> CommandAdded;

        public HistoryBoxViewModel()
        {
            Label = "History";
            MenuText = "History Box";
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["HistoryBoxIconPath"]) };
            DefaultDock = SidebarDock.Right;
            DefaultActive = false;
        }

        public override void SetTab(ITab tab)
        {
            _commandManager = tab.CommandManager;

            Commands = tab.CommandManager.Commands;

            var notifyCollection = (INotifyCollectionChanged)Commands;
            notifyCollection.CollectionChanged += Commands_CollectionChanged;
        }

        private void Commands_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                CommandAdded?.Invoke(this, e.NewItems[0]);
            }
        }

        internal void GoToCommand(IUndoRedoableCommand command) => _commandManager?.GoTo(command);
    }
}
