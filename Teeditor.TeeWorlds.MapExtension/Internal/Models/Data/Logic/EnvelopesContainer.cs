using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic
{
    internal class EnvelopesContainer
    {
        private ObservableCollection<MapEnvelope> _items;

        public ReadOnlyObservableCollection<MapEnvelope> Items { get; }

        public EnvelopesContainer()
        {
            _items = new ObservableCollection<MapEnvelope>();

            Items = new ReadOnlyObservableCollection<MapEnvelope>(_items);
        }

        public void Add(MapEnvelope envelope)
        {
            envelope.CarrierChanged += Envelope_CarrierChanged;
            _items.Add(envelope);
        }

        public void Remove(MapEnvelope envelope)
        {
            envelope.CarrierChanged -= Envelope_CarrierChanged;
            _items.Remove(envelope);
        }

        public bool TryGet(int index, out MapEnvelope envelope)
        {
            if (index < 0 || index >= _items.Count)
            {
                envelope = null;
                return false;
            }

            envelope = _items[index];
            return true;
        }

        private void Envelope_CarrierChanged(object sender, EnvelopeCarrierChangedEventArgs e)
        {
            var envelope = (MapEnvelope)sender;

            if (envelope == null)
                return;

            if (e.NewCarrier != null)
            {
                if (e.NewCarrier is MapTilesLayer tilesLayer)
                {
                    tilesLayer.ColorEnvelopeId = Items.IndexOf(envelope);
                }
            }
            else if (e.OldCarrier != null)
            {
                if (e.OldCarrier is MapTilesLayer tilesLayer)
                {
                    tilesLayer.ColorEnvelopeId = -1;
                }
            }
        }
    }
}
