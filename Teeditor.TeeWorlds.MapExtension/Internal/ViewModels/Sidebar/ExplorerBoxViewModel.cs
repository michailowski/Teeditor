using System.Collections.ObjectModel;
using Teeditor.Common;
using Teeditor.Common.Utilities;
using Teeditor.Common.ViewModels;
using Teeditor.TeeWorlds.MapExtension.Internal.Views.Sidebar;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Factories;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Models.Sidebar;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar
{
    internal class ExplorerBoxViewModel : BoxViewModelBase
    {
        private Map _map;
        private SidebarManager _sidebarManager;
        private GroupedLayersContainer _groupedLayersContainer;
        private MapTilesLayerFactory _tilesLayerFactory;
        private MapQuadsLayerFactory _quadsLayerFactory;

        public GroupedLayersContainer GroupedLayersContainer
        {
            get => _groupedLayersContainer;
            set => Set(ref _groupedLayersContainer, value);
        }

        public GroupLayerPair Selection => _map.CurrentExplorerSelection;

        public ExplorerBoxViewModel()
        {
            Label = "Explorer";
            MenuText = "Explorer Box";
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["ExplorerBoxIconPath"]) };
            Dock = SidebarDock.Left;

            _tilesLayerFactory = new MapTilesLayerFactory();
            _quadsLayerFactory = new MapQuadsLayerFactory();
        }

        public override void SetTab(ITab tab)
        {
            _map = tab?.Data as Map;
            _sidebarManager = tab?.SidebarManager as SidebarManager;

            GroupedLayersContainer = _map?.GroupedLayersContainer;
        }

        public void OpenPropertiesBox(MapItem mapItem)
        {
            _sidebarManager.PropertyBoxItem = mapItem;
            _sidebarManager?.TryOpen(typeof(PropertiesBoxControl));
        }

        public void OpenMapPropertiesBox()
        {
            _sidebarManager.PropertyBoxItem = _map.Info;
            _sidebarManager?.TryOpen(typeof(PropertiesBoxControl));
        }

        public void MinimizeOtherGroups(MapGroup myGroup)
        {
            foreach (var group in GroupedLayersContainer.Groups)
            {
                if (myGroup != group)
                {
                    group.IsExpanded = true;
                }
            }
        }

        public void MinimizeAllGroups()
        {
            foreach (var group in GroupedLayersContainer.Groups)
            {
                group.IsExpanded = true;
            }
        }

        public void MaximizeAllGroups()
        {
            foreach (var group in GroupedLayersContainer.Groups)
            {
                group.IsExpanded = false;
            }
        }

        public void AddNewGroup()
        {
            var newGroup = new MapGroup();
            GroupedLayersContainer?.Add(newGroup);
        }

        public void RemoveGroup(MapGroup group)
        {
            if (group.IsGameGroup)
                return;

            GroupedLayersContainer?.Remove(group);
        }

        public void AddTilesLayer(MapGroup group)
        {
            var newLayer = (MapLayer)_tilesLayerFactory.Create();
            group.Add(newLayer);
        }

        public void AddQuadsLayer(MapGroup group)
        {
            var newLayer = (MapLayer)_quadsLayerFactory.Create();
            group.Add(newLayer);
        }

        public void RemoveLayer(MapLayer myLayer)
        {
            if (myLayer is MapTilesLayer selectedTilesLayer && selectedTilesLayer.IsGameLayer)
                return;

            foreach (var group in GroupedLayersContainer.Groups)
            {
                foreach (var layer in group.Layers)
                {
                    if (myLayer == layer)
                    {
                        group.Remove(myLayer);
                        return;
                    }
                }
            }
        }
    }
}
