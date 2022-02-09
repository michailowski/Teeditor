namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload
{
    internal class MapFilePayload
    {
        public MapFilePayloadItems Items { get; private set; }
        public MapFilePayloadData Data { get; private set; }

        public MapFilePayload()
        {
            Items = new MapFilePayloadItems();
            Data = new MapFilePayloadData();
        }
    }
}