using System;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic
{
    internal class EnvelopeCarrierChangedEventArgs : EventArgs
    {
        public MapLayer NewCarrier { get; private set; }
        public MapLayer OldCarrier { get; private set; }

        public EnvelopeCarrierChangedEventArgs(MapLayer newCarrier = null, MapLayer oldCarrier = null)
        {
            NewCarrier = newCarrier;
            OldCarrier = oldCarrier;
        }
    }
}
