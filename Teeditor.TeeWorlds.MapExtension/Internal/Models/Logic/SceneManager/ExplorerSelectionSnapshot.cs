using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager
{
    internal struct ExplorerSelectionSnapshot
    {
        public MapGroup Group { get; }
        public MapLayer Layer { get; }

        public bool IsEmpty => Group == null || Layer == null;

        public ExplorerSelectionSnapshot(Map map)
        {
            Group = map.CurrentExplorerSelection.Group;
            Layer = map.CurrentExplorerSelection.Layer;
        }

        public ExplorerSelectionSnapshot(MapGroup group, MapLayer layer)
        {
            Group = group;
            Layer = layer;
        }
    }
}
