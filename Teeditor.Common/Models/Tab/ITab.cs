using System.ComponentModel;
using System.Threading.Tasks;
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
    public interface ITab : INotifyPropertyChanged
    {
        string Label { get; }
        string State { get; }

        IEditableFile File { get; }
        IEditableEntity Data { get; }

        IComponentsManager ComponentsManager { get; }
        ISceneManager SceneManager { get; }
        SidebarManagerBase SidebarManager { get; }
        CommandManager CommandManager { get; }
        IModificationObserver ModificationObserver { get; }
        PropertiesManagerBase PropertiesManager { get; }
        ToolbarManagerBase ToolbarManager { get; }
        PanelbarManagerBase PanelbarManager { get; }

        Task SaveAsync();
        Task SaveAsAsync(StorageFile storagefile);
    }
}
