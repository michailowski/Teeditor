using System;
using System.Collections.ObjectModel;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic
{
    internal class GroupedLayersContainer
    {
        public ObservableCollection<MapGroup> Groups { get; }

        public event EventHandler<VisualChangedEventArgs> VisualChanged;

        public GroupedLayersContainer()
        {
            Groups = new ObservableCollection<MapGroup>();
        }

        public void Add(MapGroup group)
        {
            group.VisualChanged += Group_VisualChanged;
            VisualChanged?.Invoke(this, new VisualChangedEventArgs() { ChangedItem = this });

            Groups.Add(group);
        }

        public void Remove(MapGroup group)

        {
            group.VisualChanged -= Group_VisualChanged;
            VisualChanged?.Invoke(this, new VisualChangedEventArgs() { ChangedItem = this });

            Groups.Remove(group);
        }

        private void Group_VisualChanged(object sender, VisualChangedEventArgs e)
            => VisualChanged?.Invoke(sender, e);
    }
}
