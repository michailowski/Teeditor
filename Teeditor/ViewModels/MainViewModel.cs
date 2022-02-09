using Windows.Storage;
using Teeditor.Common.ViewModels;
using Teeditor.Models;
using System.Linq;
using System.Collections.Generic;
using Teeditor.Common.Models.IO;
using Teeditor.Common.Models.Tab;

namespace Teeditor.ViewModels
{
    internal class MainViewModel : DynamicViewModel
    {
        private TabsContainer _tabsContainer;

        public bool IsLoading => _tabsContainer.IsLoading;

        public MainViewModel(TabsContainer tabsContainer)
        {
            _tabsContainer = tabsContainer;

            DynamicModel = tabsContainer;
        }

        public void CreateProject(ProjectType projectType)
            => _tabsContainer.Create(projectType);

        public void OpenProject(StorageFile storageFile)
            => _tabsContainer.Create(storageFile);

        public IEnumerable<ITab> GetModifiedTabs()
            => _tabsContainer.Items.Where(x => x.ModificationObserver.IsModified == true);
    }
}