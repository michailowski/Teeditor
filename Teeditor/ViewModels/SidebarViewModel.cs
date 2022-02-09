using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teeditor.Common.Views.Sidebar;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Models.Sidebar;
using Teeditor.Common.Models.Bindable;

namespace Teeditor.ViewModels
{
    internal class SidebarViewModel : BindableBase
    {
        private SidebarManagerBase _sidebarManager;

        public ReadOnlyObservableCollection<BoxControl> Boxes 
            => new ReadOnlyObservableCollection<BoxControl>(_sidebarManager?.Items);

        internal event EventHandler TabUpdated;

        public event EventHandler<SidebarItemChangedEventArgs> BoxDockChanged;
        public event EventHandler<SidebarItemChangedEventArgs> BoxOrderChanged;
        public event EventHandler<SidebarItemChangedEventArgs> BoxVisibilityChanged;
        public event EventHandler<SidebarItemChangedEventArgs> BoxDragStarted;
        public event EventHandler<SidebarItemChangedEventArgs> BoxDragEnded;

        public void SetTab(ITab tab)
        {
            if (_sidebarManager == tab?.SidebarManager)
                return;

            if (_sidebarManager != null)
            {
                _sidebarManager.ItemDockChanged -= SidebarManager_ItemDockChanged;
                _sidebarManager.ItemOrderChanged -= SidebarManager_ItemOrderChanged;
                _sidebarManager.ItemVisibilityChanged -= SidebarManager_ItemVisibilityChanged;
                _sidebarManager.ItemDragStarted -= SidebarManager_ItemDragStarted;
                _sidebarManager.ItemDragEnded -= SidebarManager_ItemDragEnded;
            }

            _sidebarManager = tab?.SidebarManager;
            _sidebarManager?.SetTab(tab);

            if (_sidebarManager != null)
            {
                _sidebarManager.ItemDockChanged += SidebarManager_ItemDockChanged;
                _sidebarManager.ItemOrderChanged += SidebarManager_ItemOrderChanged;
                _sidebarManager.ItemVisibilityChanged += SidebarManager_ItemVisibilityChanged;
                _sidebarManager.ItemDragStarted += SidebarManager_ItemDragStarted;
                _sidebarManager.ItemDragEnded += SidebarManager_ItemDragEnded;
            }

            TabUpdated?.Invoke(this, EventArgs.Empty);
        }

        public List<BoxControl> GetItemsByDock(SidebarDock dock) 
            => _sidebarManager?.GetItemsByDock(dock);

        public int GetActiveItemsCountByDock(SidebarDock dock) 
            => _sidebarManager?.GetActiveItemsCountByDock(dock) ?? 0;

        private void SidebarManager_ItemVisibilityChanged(object sender, SidebarItemChangedEventArgs e)
            => BoxVisibilityChanged?.Invoke(this, e);

        private void SidebarManager_ItemOrderChanged(object sender, SidebarItemChangedEventArgs e)
            => BoxOrderChanged?.Invoke(this, e);
        
        private void SidebarManager_ItemDockChanged(object sender, SidebarItemChangedEventArgs e) 
            => BoxDockChanged?.Invoke(this, e);

        private void SidebarManager_ItemDragEnded(object sender, SidebarItemChangedEventArgs e)
            => BoxDragEnded?.Invoke(this, e);
        
        private void SidebarManager_ItemDragStarted(object sender, SidebarItemChangedEventArgs e)
            => BoxDragStarted?.Invoke(this, e);
    }
}
