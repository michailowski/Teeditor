using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Views.Toolbar;
using Teeditor.ViewModels;

namespace Teeditor.Common.Models.Toolbar
{
    public abstract class ToolbarManagerBase : IToolbarManager
    {
        public ObservableCollection<ToolControl> Items { get; } = new ObservableCollection<ToolControl>();

        public event EventHandler<ToolbarItemChangedEventArgs> ItemOrderChanged;

        public ToolbarManagerBase()
        {
            Items.CollectionChanged += Items_CollectionChanged;

            AddCommonItems();
        }

        private void AddCommonItems()
        {
            var commandToolViewModel = new CommandToolViewModel();
            Items.Add(new CommandToolControl(commandToolViewModel));
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
                foreach (ToolControl tool in e.NewItems)
                {
                    tool.DropToLeftNeeded += Tool_DropToLeftNeeded;
                    tool.DropToRightNeeded += Tool_DropToRightNeeded;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (ToolControl tool in e.OldItems)
                {
                    tool.DropToLeftNeeded -= Tool_DropToLeftNeeded;
                    tool.DropToRightNeeded -= Tool_DropToRightNeeded;
                }
            }
        }

        private void Tool_DropToRightNeeded(object sender, ToolControl e)
        {
            var tool = (ToolControl)sender;

            Items.Remove(e);

            var toolIndex = Items.IndexOf(tool);

            Items.Insert(toolIndex + 1, e);

            ItemOrderChanged?.Invoke(this, new ToolbarItemChangedEventArgs(e));
        }

        private void Tool_DropToLeftNeeded(object sender, ToolControl e)
        {
            var tool = (ToolControl)sender;

            Items.Remove(e);

            var toolIndex = Items.IndexOf(tool);

            Items.Insert(toolIndex, e);

            ItemOrderChanged?.Invoke(this, new ToolbarItemChangedEventArgs(e));
        }
    }
}
