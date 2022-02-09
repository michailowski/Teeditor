using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI.Xaml;
using Teeditor.Common.Helpers;
using Teeditor.Common.Views.Sidebar;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Models.Bindable;

namespace Teeditor.Common.Models.Sidebar
{
    public enum SidebarDock { Left, Right }

    public class SidebarItemChangedEventArgs : EventArgs
    {
        public BoxControl Box { get; }

        public SidebarItemChangedEventArgs(BoxControl box)
        {
            Box = box;
        }
    }

    public class SidebarManagerBase : BindableBase
    {
        public ObservableCollection<BoxControl> Items { get; } = new ObservableCollection<BoxControl>();

        public event EventHandler<SidebarItemChangedEventArgs> ItemDockChanged;
        public event EventHandler<SidebarItemChangedEventArgs> ItemOrderChanged;
        public event EventHandler<SidebarItemChangedEventArgs> ItemVisibilityChanged;
        public event EventHandler<SidebarItemChangedEventArgs> ItemDragStarted;
        public event EventHandler<SidebarItemChangedEventArgs> ItemDragEnded;

        public SidebarManagerBase()
        {
            Items.CollectionChanged += Items_CollectionChanged;
        }

        public void SetTab(ITab tab)
        {
            foreach (var item in Items)
            {
                item.ViewModel?.SetTab(tab);
            }
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (BoxControl box in e.NewItems)
                {
                    box.VisibilityChanged += Box_VisibilityChanged;
                    box.DockToggleNeeded += Box_DockToggleNeeded;
                    box.MoveUpNeeded += Box_MoveUpNeeded;
                    box.MoveDownNeeded += Box_MoveDownNeeded;
                    box.DropToUpNeeded += Box_DropToUpNeeded;
                    box.DropToDownNeeded += Box_DropToDownNeeded;
                    box.DragStarting += Box_DragStarting;
                    box.DropCompleted += Box_DropCompleted;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (BoxControl box in e.OldItems)
                {
                    box.VisibilityChanged -= Box_VisibilityChanged;
                    box.DockToggleNeeded -= Box_DockToggleNeeded;
                    box.MoveUpNeeded -= Box_MoveUpNeeded;
                    box.MoveDownNeeded -= Box_MoveDownNeeded;
                    box.DropToUpNeeded -= Box_DropToUpNeeded;
                    box.DropToDownNeeded -= Box_DropToDownNeeded;
                    box.DragStarting -= Box_DragStarting;
                    box.DropCompleted -= Box_DropCompleted;
                }
            }
        }

        private void Box_VisibilityChanged(object sender, EventArgs e)
            => ItemVisibilityChanged?.Invoke(this, new SidebarItemChangedEventArgs(sender as BoxControl));

        private void Box_DropCompleted(UIElement sender, DropCompletedEventArgs args)
            => ItemDragEnded?.Invoke(this, new SidebarItemChangedEventArgs(sender as BoxControl));

        private void Box_DragStarting(UIElement sender, DragStartingEventArgs args)
            => ItemDragStarted?.Invoke(this, new SidebarItemChangedEventArgs(sender as BoxControl));

        private void Box_DropToUpNeeded(object sender, BoxControl e)
        {
            var box = (BoxControl)sender;

            Items.Remove(e);

            var boxIndex = Items.IndexOf(box);

            Items.Insert(boxIndex, e);

            ItemOrderChanged?.Invoke(this, new SidebarItemChangedEventArgs(e));
        }

        private void Box_DropToDownNeeded(object sender, BoxControl e)
        {
            var box = (BoxControl)sender;

            Items.Remove(e);

            var boxIndex = Items.IndexOf(box);

            Items.Insert(boxIndex + 1, e);

            ItemOrderChanged?.Invoke(this, new SidebarItemChangedEventArgs(e));
        }

        private void Box_MoveDownNeeded(object sender, EventArgs e)
        {
            var box = (BoxControl)sender;

            var sidebarItemIndex = Items.IndexOf(box);

            if (sidebarItemIndex == Items.Count - 1)
                return;

            for (int i = sidebarItemIndex + 1; i < Items.Count; i++)
            {
                if (Items[i].ViewModel.Dock == box.ViewModel.Dock)
                {
                    (Items[i], Items[sidebarItemIndex]) = (Items[sidebarItemIndex], Items[i]);
                    ItemOrderChanged?.Invoke(this, new SidebarItemChangedEventArgs(box));

                    break;
                }
            }
        }

        private void Box_MoveUpNeeded(object sender, EventArgs e)
        {
            var box = (BoxControl)sender;

            var sidebarItemIndex = Items.IndexOf(box);

            if (sidebarItemIndex <= 0)
                return;

            for (int i = sidebarItemIndex - 1; i >= 0; i--)
            {
                if (Items[i].ViewModel.Dock == box.ViewModel.Dock)
                {
                    (Items[i], Items[sidebarItemIndex]) = (Items[sidebarItemIndex], Items[i]);
                    ItemOrderChanged?.Invoke(this, new SidebarItemChangedEventArgs(box));

                    break;
                }
            }
        }

        private void Box_DockToggleNeeded(object sender, EventArgs e)
        {
            var box = (BoxControl)sender;

            var boxParent = box.Parent;
            boxParent.RemoveChild(box);

            ItemDockChanged?.Invoke(this, new SidebarItemChangedEventArgs(box));
        }

        public List<BoxControl> GetItemsByDock(SidebarDock dock)
            => Items.Where(x => x.ViewModel.Dock == dock).ToList();

        public int GetActiveItemsCountByDock(SidebarDock dock)
            => Items.Count(x => x.Visibility == Visibility.Visible && x.ViewModel.Dock == dock);

        public void TryOpen(Type boxType)
        {
            var box = Items.FirstOrDefault(x => x.GetType() == boxType);

            if (box == null)
                return;

            box.Visibility = Visibility.Visible;
        }
    }
}
