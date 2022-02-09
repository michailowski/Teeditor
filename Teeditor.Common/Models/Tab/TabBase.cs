using System.IO;
using System.Threading.Tasks;
using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Models.Commands;
using Teeditor.Common.Models.Components;
using Teeditor.Common.Models.IO;
using Teeditor.Common.Models.ModificationObserving;
using Teeditor.Common.Models.Panelbar;
using Teeditor.Common.Models.Properties;
using Teeditor.Common.Models.Scene;
using Teeditor.Common.Models.Sidebar;
using Teeditor.Common.Models.Toolbar;
using Windows.Storage;

namespace Teeditor.Common.Models.Tab
{
    public abstract class TabBase : BindableBase, ITab
    {
        private string _label;
        private string _state;
        private ModificationObserver _modificationObserver;

        public string Label
        {
            get => _label;
            protected set => Set(ref _label, value);
        }

        public string State
        {
            get => _state;
            protected set => Set(ref _state, value);
        }

        public IEditableFile File { get; private set; }

        public IEditableEntity Data { get; private set; }

        public IComponentsManager ComponentsManager => GetComponentsManager();

        public ISceneManager SceneManager => GetSceneManager();

        public SidebarManagerBase SidebarManager => GetSidebarManager();

        public ToolbarManagerBase ToolbarManager => GetToolbarManager();

        public PanelbarManagerBase PanelbarManager => GetPanelbarManager();

        public PropertiesManagerBase PropertiesManager => GetPropertiesManager();

        public CommandManager CommandManager { get; } = new CommandManager();

        public IModificationObserver ModificationObserver => _modificationObserver;

        public TabBase(IEditableFile editableFile, EditableEntityBase editableEntity)
        {
            File = editableFile;
            Data = editableEntity;
            Label = editableFile.Name;
            State = "Ready";

            _modificationObserver = new ModificationObserver(CommandManager);
            _modificationObserver.Add(Data, GetEditableDataObservingStrategy());
        }

        public async Task SaveAsync()
        {
            if (File.IsStored == false)
                return;
            
            State = "Saving is started";
            await File.SaveAsync(Data);
            State = "Successfully saved";

            _modificationObserver.IsModified = false;

            await Task.Delay(2000).ContinueWith(t => { State = "Ready"; });
        }

        public async Task SaveAsAsync(StorageFile storagefile)
        {
            State = "Saving is started";
            await File.SaveAsAsync(Data, storagefile);
            State = $"Successfully saved into \"{storagefile.Path}\"";

            Label = Path.GetFileNameWithoutExtension(storagefile.Name);
            _modificationObserver.IsModified = false;

            await Task.Delay(2000).ContinueWith(t => { State = "Ready"; });
        }

        protected abstract IComponentsManager GetComponentsManager();
        protected abstract ISceneManager GetSceneManager();
        protected abstract SidebarManagerBase GetSidebarManager();
        protected abstract PropertiesManagerBase GetPropertiesManager();
        protected abstract ToolbarManagerBase GetToolbarManager();
        protected abstract PanelbarManagerBase GetPanelbarManager();
        protected abstract ModificationObservingStrategyBase GetEditableDataObservingStrategy();
    }
}
