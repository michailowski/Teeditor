
namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic
{
    internal class LayersSelectionsManager
    {
        public TilesLayerSelection TilesLayerSelection { get; private set; }
        public QuadsLayerSelection QuadsLayerSelection { get; private set; }

        public LayersSelectionsManager()
        {
            TilesLayerSelection = new TilesLayerSelection();
            QuadsLayerSelection = new QuadsLayerSelection();
        }
    }
}
