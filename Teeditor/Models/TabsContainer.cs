using System;
using System.Collections.ObjectModel;
using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Models.IO;
using Teeditor.Common.Models.Tab;
using Windows.Storage;

namespace Teeditor.Models
{
    internal class TabsContainer : BindableBase
    {
        private ITab _selectedTab = null;
        private bool _isLoading = false;
        private TabBuilder _tabBuilder;

        public ObservableCollection<ITab> Items { get; } = new ObservableCollection<ITab>();

        public TabsContainer()
        {
            _tabBuilder = new TabBuilder();

            _tabBuilder.TabBuildingStarted += TabBuilder_TabBuildingStarted;
            _tabBuilder.TabBuildingEnded += TabBuilder_TabBuildingEnded;
        }

        public ITab SelectedTab
        {
            get => _selectedTab;
            set => Set(ref _selectedTab, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set => Set(ref _isLoading, value);
        }

        public void Create(ProjectType projectType)
            => _tabBuilder.Create(projectType);

        public void Create(StorageFile storageFile)
            => _tabBuilder.Create(storageFile);

        public void Close(ITab tab)
            => Items.Remove(tab);

        private void TabBuilder_TabBuildingStarted(object sender, EventArgs e)
            => IsLoading = true;

        private void TabBuilder_TabBuildingEnded(object sender, TabBuildingEndedEventArgs e)
        {
            IsLoading = e.IsQueueEmpty == false;

            if (e.CreatedTab == null)
                return;

            Items.Add(e.CreatedTab);
            SelectedTab = Items[Items.Count - 1];
        }
    }
}
