using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Models.IO;
using Teeditor.Common.Models.Tab;
using Teeditor.Models;
using Teeditor.ViewModels.Dialogs;
using Windows.Storage;

namespace Teeditor.ViewModels.Toolbar.Tools
{
    internal class MainToolViewModel : BindableBase
    {
        private TabsContainer _tabsContainer;

        public ProjectCreateDialogViewModel CreateProjectDialogViewModel { get; }
        public bool IsFileStored => _tabsContainer.SelectedTab.File.IsStored;

        public MainToolViewModel(TabsContainer tabsContainer)
        {
            _tabsContainer = tabsContainer;

            CreateProjectDialogViewModel = new ProjectCreateDialogViewModel();
        }

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
    }
}
