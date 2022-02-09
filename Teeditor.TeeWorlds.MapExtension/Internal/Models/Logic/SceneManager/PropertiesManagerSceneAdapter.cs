using Teeditor.Common.Models.Properties;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager
{
    internal class PropertiesManagerSceneAdapter
    {
        private PropertiesManagerBase _propertiesManager;

        public bool IsHighDetailEnabled => _propertiesManager?.GetProperty<bool>().Value ?? true;
        public bool IsGridEnabled => _propertiesManager?.GetProperty<bool>().Value ?? false;
        public bool IsProofBordersEnabled => _propertiesManager?.GetProperty<bool>().Value ?? false;

        public void SetManager(PropertiesManagerBase propertiesManager)
            => _propertiesManager = propertiesManager;
    }
}
