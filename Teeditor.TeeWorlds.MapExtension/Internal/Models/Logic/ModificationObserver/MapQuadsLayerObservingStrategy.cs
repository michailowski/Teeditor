using System;
using System.Collections.Specialized;
using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Models.ModificationObserving;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.ModificationObserver
{
    internal class MapQuadsLayerObservingStrategy : ModificationObservingStrategyBase
    {
        private MapQuadsLayer QuadsLayer => _observableModel as MapQuadsLayer;

        private static string[] _propertiesNames
            = { "Name", "Image", "IsHighDetail" };

        protected override void Initialize()
        {
            QuadsLayer.PropertyModificated += QuadsLayer_PropertyModificated;
            QuadsInit();
        }

        private void QuadsLayer_PropertyModificated(object sender, PropertyModificatedEventArgs e)
        {
            if (Array.IndexOf(_propertiesNames, e.PropertyName) == -1)
                return;

            RaiseModification(e);
        }

        private void QuadsInit()
        {
            foreach (MapQuad quad in QuadsLayer.Quads)
            {
                Add(quad, new MapQuadObservingStrategy());
            }

            QuadsLayer.Quads.CollectionChanged += Quads_CollectionChanged;
        }

        private void Quads_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MapQuad quad in e.NewItems)
                {
                    Add(quad, new MapQuadObservingStrategy());
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (MapQuad quad in e.OldItems)
                {
                    Remove(quad);
                }
            }

            RaiseModification(_observableModel);
        }
    }
}
