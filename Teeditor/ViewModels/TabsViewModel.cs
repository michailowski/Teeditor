using System.Collections.ObjectModel;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.ViewModels;
using Teeditor.Models;

namespace Teeditor.ViewModels
{
    internal class TabsViewModel : DynamicViewModel
    {
        private TabsContainer _tabsContainer;

        public ObservableCollection<ITab> Tabs => _tabsContainer.Items;

        public ITab SelectedTab
        {
            get => _tabsContainer.SelectedTab;
            set => _tabsContainer.SelectedTab = value;
        }

        public TabsViewModel(TabsContainer tabsContainer)
        {
            _tabsContainer = tabsContainer;

            DynamicModel = _tabsContainer;
        }

        public void CloseTab(ITab tab) => _tabsContainer.Close(tab);
    }
}
