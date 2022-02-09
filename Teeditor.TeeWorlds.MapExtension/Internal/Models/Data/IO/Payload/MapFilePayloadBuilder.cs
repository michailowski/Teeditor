using System.Threading.Tasks;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Windows.Storage.Streams;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload
{
    internal class MapFilePayloadBuilder
    {
        private MapFilePayload _payload;
        private MapFilePayloadPartAddingStrategy _currentPartAddingStrategy;

        public MapFilePayloadBuilder()
            => _payload = new MapFilePayload();

        public void SetPartAddingStrategy(MapFilePayloadPartAddingStrategy strategy)
        {
            _currentPartAddingStrategy = strategy;
            _currentPartAddingStrategy.SetData(_payload);
        }

        public async Task<IMapItemDTO> AddPartAsync(MapItem mapItem)
            => await _currentPartAddingStrategy?.AddAsync(mapItem);

        public IMapItemDTO AddPart(MapItem mapItem)
            => _currentPartAddingStrategy?.Add(mapItem);

        public IMapItemDTO AddPart(DataReader dataReader)
            => _currentPartAddingStrategy?.Add(dataReader);

        public IMapItemDTO AddPart(DataReader dataReader, int version)
            => _currentPartAddingStrategy?.Add(dataReader, version);

        public void AddPart(IMapItemDTO mapItemDTO)
            => _currentPartAddingStrategy?.Add(mapItemDTO);

        public async Task<bool> TryAddDataAsync(int index, byte[] data)
            => await _payload.Data.TryAddCompressedAsync(index, data);

        public MapFilePayload GetResult() => _payload;
    }
}
