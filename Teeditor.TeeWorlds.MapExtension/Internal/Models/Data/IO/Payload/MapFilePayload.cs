using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload
{
    internal class MapFilePayload
    {
        public PayloadType Type { get; private set; }
        public MapFilePayloadItems Items { get; private set; }
        public MapFilePayloadData Data { get; private set; }

        public MapFilePayload(PayloadType type)
        {
            Type = type;
            Items = new MapFilePayloadItems();
            Data = new MapFilePayloadData();
        }
    }
}