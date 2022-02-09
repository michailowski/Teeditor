using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Teeditor.Common.Views.Sidebar;
using Teeditor.Common.Views.Toolbar;
using Teeditor.Common.ViewModels;
using Teeditor.Models;
using System.Threading.Tasks;
using Windows.Storage;
using System.Collections.Specialized;
using Teeditor.Common.Models.Commands;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Models.Panelbar;
using Teeditor.Common.Models.Properties;
using Teeditor.Common.Models.IO;
using System.Linq;

namespace Teeditor.ViewModels
{
    internal class MainMenuViewModel : DynamicViewModel
    {
        private TabsContainer _tabsContainer;
        private CommandManager _commandManager;
        private bool _isSaveAllowed;

        public ReadOnlyObservableCollection<BoxControl> Boxes { get; private set; }
        public ReadOnlyObservableCollection<ToolControl> Tools { get; private set; }
        public ReadOnlyObservableCollection<PanelItem> Panels { get; private set; }
        public IReadOnlyDictionary<string, IPropertiesItem> Properties { get; private set; }

        public bool IsUndoAllowed => _commandManager?.IsUndoAllowed ?? false;
        public bool IsRedoAllowed => _commandManager?.IsRedoAllowed ?? false;

        public bool IsSaveAllowed
        {
            get => _isSaveAllowed;
            private set => Set(ref _isSaveAllowed, value);
        }

        internal event EventHandler TabUpdated;

        public MainMenuViewModel(TabsContainer tabsContainer)
        {
            _tabsContainer = tabsContainer;
            _tabsContainer.PropertyChanged += TabsContainer_PropertyChanged;
        }

        public void SetTab(ITab tab)
        {
            if (tab == null)
            {
                Boxes = null;
                Tools = null;
                Panels = null;
                Properties = null;
            }
            else
            {
                Boxes = new ReadOnlyObservableCollection<BoxControl>(tab?.SidebarManager?.Items);
                Tools = new ReadOnlyObservableCollection<ToolControl>(tab?.ToolbarManager?.Items);
                Panels = new ReadOnlyObservableCollection<PanelItem>(tab?.PanelbarManager?.Items);
                Properties = tab?.PropertiesManager?.Items;
            }

            _commandManager = tab?.CommandManager;

            RaisePropertiesChange();

            TabUpdated?.Invoke(this, EventArgs.Empty);

            DynamicModel = _commandManager;
        }

        public void Undo() => _commandManager?.Undo();
        
        public void Redo() => _commandManager?.Redo();


        public void CreateProject(ProjectType projectType)
            => _tabsContainer.Create(projectType);

        public void OpenProject(StorageFile storageFile)
            => _tabsContainer.Create(storageFile);

        public async Task SaveProjectAsync()
            => await _tabsContainer.SelectedTab.SaveAsync();
        
        public async Task SaveProjectAsAsync(StorageFile storageFile)
            => await _tabsContainer.SelectedTab.SaveAsAsync(storageFile);

        public IEnumerable<ITab> GetModifiedTabs()
            => _tabsContainer.Items.Where(x => x.ModificationObserver.IsModified == true);

        public string GetCurrentExtension()
            => _tabsContainer.SelectedTab.File.Extension;

        public string GetCurrentFileName()
            => _tabsContainer.SelectedTab.File.Name;

        private void TabsContainer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "SelectedTab")
                return;

            IsSaveAllowed = _tabsContainer.SelectedTab != null;
        }

        private void RaisePropertiesChange()
        {
            OnPropertyChanged("IsRedoAllowed");
            OnPropertyChanged("IsUndoAllowed");
        }
    }
}
