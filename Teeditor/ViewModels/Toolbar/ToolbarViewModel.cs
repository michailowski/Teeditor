using System;
using System.Collections.ObjectModel;
using Teeditor.Common.Views.Toolbar;
using Teeditor.ViewModels.Toolbar.Tools;
using Teeditor.Models;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Models.Toolbar;
using Teeditor.Common.Models.Bindable;

namespace Teeditor.ViewModels.Toolbar
{
    internal class ToolbarViewModel : BindableBase
    {
        private ToolbarManagerBase _toolbarManager;
        private ReadOnlyObservableCollection<ToolControl> _tools;

        public ReadOnlyObservableCollection<ToolControl> Tools => _tools;

        public MainToolViewModel MainToolViewModel { get; }

        public event EventHandler TabUpdated;

        public event EventHandler<ToolbarItemChangedEventArgs> ToolOrderChanged;

        public ToolbarViewModel(TabsContainer tabsContainer)
        {
            MainToolViewModel = new MainToolViewModel(tabsContainer);
        }

        public void SetTab(ITab tab)
        {
            if (_toolbarManager == tab?.ToolbarManager)
                return;

            if (_toolbarManager != null)
            {
                _toolbarManager.ItemOrderChanged -= ToolbarManager_ItemOrderChanged;
                _tools = null;
            }

            _toolbarManager = tab?.ToolbarManager;

            if (_toolbarManager != null)
            {
                _toolbarManager.ItemOrderChanged += ToolbarManager_ItemOrderChanged;
                _tools = new ReadOnlyObservableCollection<ToolControl>(_toolbarManager?.Items);
            }

            TabUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void ToolbarManager_ItemOrderChanged(object sender, ToolbarItemChangedEventArgs e)
            => ToolOrderChanged?.Invoke(this, e);
    }
}
