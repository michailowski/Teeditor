using System;
using System.Collections.ObjectModel;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic
{
    internal class GroupedLayersContainer
    {
        private ObservableCollection<MapGroup> _groups;

        public ReadOnlyObservableCollection<MapGroup> Groups { get; }

        public event EventHandler<VisualChangedEventArgs> VisualChanged;

        public GroupedLayersContainer()
        {
            _groups = new ObservableCollection<MapGroup>();

            Groups = new ReadOnlyObservableCollection<MapGroup>(_groups);
        }

        public void Add(MapGroup group)
        {
            group.VisualChanged += Group_VisualChanged;
            VisualChanged?.Invoke(this, new VisualChangedEventArgs() { ChangedItem = this });

            _groups.Add(group);
        }

        public void Remove(MapGroup group)
        {
            group.VisualChanged -= Group_VisualChanged;
            VisualChanged?.Invoke(this, new VisualChangedEventArgs() { ChangedItem = this });

            _groups.Remove(group);
        }

        private void Group_VisualChanged(object sender, VisualChangedEventArgs e)
            => VisualChanged?.Invoke(sender, e);
    }
}
