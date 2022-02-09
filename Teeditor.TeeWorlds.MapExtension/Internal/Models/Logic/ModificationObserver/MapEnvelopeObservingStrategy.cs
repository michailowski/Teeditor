using System;
using System.Collections.Specialized;
using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Models.ModificationObserving;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.ModificationObserver
{
    internal class MapEnvelopeObservingStrategy : ModificationObservingStrategyBase
    {
        private MapEnvelope Envelope => _observableModel as MapEnvelope;

        private static string[] _propertiesNames = { "Name", "IsSynchronized" };

        protected override void Initialize()
        {
            Envelope.PropertyModificated += Envelope_PropertyModificated;
            PointsInit();
        }

        private void Envelope_PropertyModificated(object sender, PropertyModificatedEventArgs e)
        {
            if (Array.IndexOf(_propertiesNames, e.PropertyName) == -1)
                return;

            RaiseModification(e);
        }

        private void PointsInit()
        {
            foreach (MapEnvelopePoint point in Envelope.Points)
            {
                if (point is MapEnvelopePointColor colorPoint)
                {
                    Add(colorPoint, new MapEnvPointColorObservingStrategy());
                }
                else if (point is MapEnvelopePointPosition positionPoint)
                {
                    Add(positionPoint, new MapEnvPointPositionObservingStrategy());
                }
            }

            Envelope.Points.CollectionChanged += Points_CollectionChanged;
        }

        private void Points_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MapEnvelopePoint point in e.NewItems)
                {
                    if (point is MapEnvelopePointColor colorPoint)
                    {
                        Add(colorPoint, new MapEnvPointColorObservingStrategy());
                    }
                    else if (point is MapEnvelopePointPosition positionPoint)
                    {
                        Add(positionPoint, new MapEnvPointPositionObservingStrategy());
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (MapEnvelopePoint point in e.OldItems)
                {
                    if (point is MapEnvelopePointColor colorPoint)
                    {
                        Remove(colorPoint);
                    }
                    else if (point is MapEnvelopePointPosition positionPoint)
                    {
                        Remove(positionPoint);
                    }
                }
            }

            RaiseModification(_observableModel);
        }
    }
}
