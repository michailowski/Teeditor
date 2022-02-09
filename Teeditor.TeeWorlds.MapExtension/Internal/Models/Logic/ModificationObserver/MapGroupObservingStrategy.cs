using System;
using System.Collections.Specialized;
using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Models.ModificationObserving;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.ModificationObserver
{
    internal class MapGroupObservingStrategy : ModificationObservingStrategyBase
    {
        private MapGroup Group => _observableModel as MapGroup;

        private static string[] _propertiesNames = { "Name", "Offset", "Parallax", "UseClipping", "Clip" };

        protected override void Initialize()
        {
            Group.PropertyModificated += Group_PropertyModificated;
            LayersInit();
        }

        private void Group_PropertyModificated(object sender, PropertyModificatedEventArgs e)
        {
            if (Array.IndexOf(_propertiesNames, e.PropertyName) == -1)
                return;

            RaiseModification(e);
        }

        private void LayersInit()
        {
            foreach (MapLayer layer in Group.Layers)
            {
                if (layer is MapTilesLayer tilesLayer)
                {
                    Add(tilesLayer, new MapTilesLayerObservingStrategy());
                }
                else if (layer is MapQuadsLayer quadsLayer)
                {
                    Add(quadsLayer, new MapQuadsLayerObservingStrategy());
                }
            }

            (Group.Layers as INotifyCollectionChanged).CollectionChanged += Layers_CollectionChanged;
        }

        private void Layers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MapLayer layer in e.NewItems)
                {
                    if (layer is MapTilesLayer tilesLayer)
                    {
                        Add(tilesLayer, new MapTilesLayerObservingStrategy());
                    }
                    else if (layer is MapQuadsLayer quadsLayer)
                    {
                        Add(quadsLayer, new MapQuadsLayerObservingStrategy());
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (MapLayer layer in e.OldItems)
                {
                    if (layer is MapTilesLayer tilesLayer)
                    {
                        Remove(tilesLayer);
                    }
                    else if (layer is MapQuadsLayer quadsLayer)
                    {
                        Remove(quadsLayer);
                    }
                }
            }

            RaiseModification(_observableModel);
        }
    }
}
