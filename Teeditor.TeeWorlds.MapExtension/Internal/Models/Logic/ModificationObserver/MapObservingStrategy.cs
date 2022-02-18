using System.Collections.Specialized;
using Teeditor.Common.Models.ModificationObserving;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.ModificationObserver
{
    internal class MapObservingStrategy : ModificationObservingStrategyBase
    {
        private Map Map => _observableModel as Map;

        protected override void Initialize()
        {
            Add(Map.Info, new MapInfoObservingStrategy());

            ImagesInit();
            GroupsInit();
            EnvelopesInit();
        }

        private void ImagesInit()
        {
            foreach (MapImage image in Map.ImagesContainer.Items)
            {
                Add(image, new MapImageObservingStrategy());
            }

            (Map.ImagesContainer.Items as INotifyCollectionChanged).CollectionChanged += Images_CollectionChanged;
        }

        private void GroupsInit()
        {
            foreach (MapGroup group in Map.GroupedLayersContainer.Groups)
            {
                Add(group, new MapGroupObservingStrategy());
            }

            Map.GroupedLayersContainer.Groups.CollectionChanged += Groups_CollectionChanged;
        }

        private void EnvelopesInit()
        {
            foreach (MapEnvelope envelope in Map.EnvelopesContainer.Items)
            {
                Add(envelope, new MapEnvelopeObservingStrategy());
            }

            (Map.EnvelopesContainer.Items as INotifyCollectionChanged).CollectionChanged += Envelopes_CollectionChanged;
        }

        private void Images_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MapImage image in e.NewItems)
                {
                    Add(image, new MapImageObservingStrategy());
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (MapImage image in e.OldItems)
                {
                    Remove(image);
                }
            }

            RaiseModification(Map);
        }

        private void Groups_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MapGroup group in e.NewItems)
                {
                    Add(group, new MapGroupObservingStrategy());
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (MapGroup group in e.OldItems)
                {
                    Remove(group);
                }
            }

            RaiseModification(Map);
        }

        private void Envelopes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (MapEnvelope envelope in e.NewItems)
                {
                    Add(envelope, new MapEnvelopeObservingStrategy());
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (MapEnvelope envelope in e.OldItems)
                {
                    Remove(envelope);
                }
            }

            RaiseModification(Map);
        }
    }
}
