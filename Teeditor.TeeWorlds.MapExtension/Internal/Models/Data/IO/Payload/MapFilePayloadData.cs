using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload
{
    internal class MapFilePayloadData
    {
        private Dictionary<int, byte[]> _compressedData;
        private Dictionary<int, int> _compressedDataSize;

        private Dictionary<int, byte[]> _decompressedData;
        private Dictionary<int, int> _decompressedDataSize;

        public int CompressedDataNumber => _compressedData.Count;
        public int DecompressedDataNumber => _decompressedData.Count;

        public MapFilePayloadData()
        {
            _compressedData = new Dictionary<int, byte[]>();
            _compressedDataSize = new Dictionary<int, int>();

            _decompressedData = new Dictionary<int, byte[]>();
            _decompressedDataSize = new Dictionary<int, int>();
        }

        #region Get From File

        // В ридере
        public async Task<bool> TryAddCompressedAsync(int index, byte[] data)
        {
            var decompressedData = await data.DecompressDataAsync();

            var added = _decompressedData.TryAdd(index, decompressedData);

            if (added == false)
                return false;

            _decompressedDataSize.TryAdd(index, decompressedData.Length);
            _compressedDataSize.TryAdd(index, data.Length);

            return true;
        }

        // В модели
        public bool TryGetDecompressed(int index, out byte[] decompressedData)
        {
            return _decompressedData.TryGetValue(index, out decompressedData);
        }

        public bool TryGetDecompressed(int index, out string decompressedString)
        {
            decompressedString = null;

            var hasData = _decompressedData.TryGetValue(index, out var decompressedData);

            if (hasData == false)
                return false;

            decompressedString = Encoding.UTF8.GetString(decompressedData).Replace("\0", "");
            return true;
        }

        #endregion

        #region Add To File

        // в модели
        public async Task<int> TryAddDecompressedAsync(byte[] data)
        {
            var index = _compressedData.Count;
            var compressedData = await data.CompressDataAsync();

            var added = _compressedData.TryAdd(index, compressedData);

            if (added == false)
                return -1;

            _decompressedDataSize.TryAdd(index, data.Length);
            _compressedDataSize.TryAdd(index, compressedData.Length);

            return index;
        }

        public async Task<int> TryAddDecompressedAsync(string str)
            => await TryAddDecompressedAsync(Encoding.UTF8.GetBytes(str + "\0"));

        // во врайтере
        public bool TryGetCompressed(int index, out byte[] compressedData, out int compressedDataSize, out int decompressedDataSize)
        {
            compressedDataSize = decompressedDataSize = 0;
            var hasData = _compressedData.TryGetValue(index, out compressedData);

            if (hasData == false)
                return false;

            _compressedDataSize.TryGetValue(index, out compressedDataSize);
            _decompressedDataSize.TryGetValue(index, out decompressedDataSize);

            return true;
        }

        #endregion
    }
}
