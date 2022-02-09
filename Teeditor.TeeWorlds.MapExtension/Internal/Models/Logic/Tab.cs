using Teeditor.Common.Models.Components;
using Teeditor.Common.Models.ModificationObserving;
using Teeditor.Common.Models.Panelbar;
using Teeditor.Common.Models.Properties;
using Teeditor.Common.Models.Scene;
using Teeditor.Common.Models.Sidebar;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Models.Toolbar;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.ModificationObserver;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic
{
    internal class Tab : TabBase
    {
        private static ComponentsManager _componentsManager;
        private MapSceneManager _sceneManager;
        private static ToolbarManager _toolbarManager;
        private SidebarManager _sidebarManager;
        private static PanelbarManager _panelbarManager;
        private PropertiesManager _propertiesManager;

        private MapObservingStrategy _observingStrategy;

        public Tab(MapFile file, Map map) : base(file, map)
        {
            _componentsManager = _componentsManager ?? new ComponentsManager();
            _toolbarManager = _toolbarManager ?? new ToolbarManager();
            _propertiesManager = _propertiesManager ?? new PropertiesManager();
            _sidebarManager = _sidebarManager ?? new SidebarManager();
            _panelbarManager = _panelbarManager ?? new PanelbarManager();
            _observingStrategy = _observingStrategy ?? new MapObservingStrategy();
        }

        protected override IComponentsManager GetComponentsManager()
            => _componentsManager;

        protected override ISceneManager GetSceneManager()
        {
            _sceneManager = _sceneManager ?? new MapSceneManager();

            _sceneManager.SetTab(this);

            return _sceneManager;
        }

        protected override ToolbarManagerBase GetToolbarManager()
        {
            _toolbarManager.SetTab(this);

            return _toolbarManager;
        }

        protected override PropertiesManagerBase GetPropertiesManager()
            => _propertiesManager;

        protected override SidebarManagerBase GetSidebarManager()
            => _sidebarManager;

        protected override PanelbarManagerBase GetPanelbarManager()
        {
            _panelbarManager.SetTab(this);

            return _panelbarManager;
        }

        protected override ModificationObservingStrategyBase GetEditableDataObservingStrategy()
        {
            _observingStrategy = _observingStrategy ?? new MapObservingStrategy();

            return _observingStrategy;
        }
    }
}