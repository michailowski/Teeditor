using System;
using System.Linq;
using System.Threading.Tasks;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Windows.Storage.Streams;
using Teeditor.Common.Utilities;
using Windows.Storage;
using System.Collections.Generic;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload.Strategies;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO
{
    internal static class MapFileReader
    {
        private static IRandomAccessStream _sourceStream;
        private static MapFileHeader _header;
        private static InputItemInfo[] _itemsInfo;
        private static int[] _itemsOffsets;
        private static int[] _dataOffsets;

        private static MapFilePayloadBuilder _payloadBuilder;

        private const int DatafileItemSize = sizeof(int) * 2; // Type_and_id and Size

        public static async Task<MapFilePayload> ReadAsync(StorageFile storageFile)
        {
            _sourceStream = await storageFile.OpenAsync(FileAccessMode.Read);

            _payloadBuilder = new MapFilePayloadBuilder(PayloadType.Loading);

            using (var inputStream = _sourceStream.GetInputStreamAt(0))
            {
                using (var dr = new DataReader(inputStream))
                {
                    dr.UnicodeEncoding = UnicodeEncoding.Utf8;
                    dr.ByteOrder = ByteOrder.LittleEndian;

                    await dr.LoadAsync((uint)_sourceStream.Size);

                    ReadSignature(dr);
                    ReadHeader(dr);
                    ReadItemsInfo(dr);
                    ReadItemsOffsets(dr);
                    ReadDataOffsets(dr);
                    await ReadDataAsync();
                }

                await ReadItemsAsync();
            }

            _sourceStream.Dispose();
            _sourceStream = null;

            return _payloadBuilder.GetResult();
        }

        private static void ReadSignature(DataReader dr)
        {
            string fileSignature = dr.ReadString(4);

            if (fileSignature != "DATA" && fileSignature != "ATAD")
            {
                throw new Exception("Error during file reading", new Exception("Incorrect file signature"));
            }
        }

        private static void ReadHeader(DataReader dr)
        {
            _header = new MapFileHeader();

            _header.Version = dr.ReadInt32();
            _header.Size = dr.ReadInt32();
            _header.SwapLength = dr.ReadInt32();
            _header.ItemTypesNumber = dr.ReadInt32();
            _header.ItemsNumber = dr.ReadInt32();
            _header.DataNumber = dr.ReadInt32();
            _header.ItemsSequenceSize = dr.ReadInt32();
            _header.DataSequenceSize = dr.ReadInt32();

            if (!_header.IsValidVersion)
            {
                throw new Exception("Error during file reading", new Exception("Wrong map version"));
            }
        }

        private static void ReadItemsInfo(DataReader dr)
        {
            _itemsInfo = new InputItemInfo[_header.ItemTypesNumber];

            for (int i = 0; i < _header.ItemTypesNumber; i++)
            {
                var itemType = dr.ReadInt32();
                var startIndex = dr.ReadInt32();
                var number = dr.ReadInt32();

                _itemsInfo[i] = new InputItemInfo((ItemType)itemType, startIndex, number);
            }
        }

        private static void ReadItemsOffsets(DataReader dr)
            => _itemsOffsets = dr.ReadInt32Array(_header.ItemsNumber);
        
        private static void ReadDataOffsets(DataReader dr)
            => _dataOffsets = dr.ReadInt32Array(_header.DataNumber);

        private static async Task ReadDataAsync()
        {
            for (int i = 0; i < _dataOffsets.Count(); i++)
            {
                var size = GetDataSize(i);

                if (size == 0)
                    continue;

                byte[] data = new byte[size];

                uint offset = (uint)(_header.Size + _header.ItemsSequenceSize + _dataOffsets[i]);

                using (var inputStream = _sourceStream.GetInputStreamAt(offset))
                using (var dr = new DataReader(inputStream))
                {
                    dr.UnicodeEncoding = UnicodeEncoding.Utf8;
                    dr.ByteOrder = ByteOrder.LittleEndian;

                    await dr.LoadAsync((uint)size);

                    dr.ReadBytes(data);
                }

                await _payloadBuilder.TryAddDataAsync(i, data);
            }
        }

        private static async Task ReadItemsAsync()
        {
            List<Task> tasks = new List<Task>();

            tasks.Add(ReadMapInfoAsync());
            tasks.Add(ReadMapImagesAsync());
            tasks.Add(ReadMapGroupsAsync());
            tasks.Add(ReadMapLayersAsync());
            tasks.Add(ReadMapEnvelopesAndPointsAsync());

            await Task.WhenAll(tasks);
        }

        private static async Task ReadMapInfoAsync()
        {
            var itemInfo = GetItemInfo(ItemType.Info);

            if (itemInfo == null)
                return;

            var infoAddingStrategy = new MapFilePayloadInfoAddingStrategy();

            var offset = (ulong)(_header.Size + _itemsOffsets[itemInfo.StartIndex] + DatafileItemSize); // Skip type_and_id and size

            using (var inputStream = _sourceStream.GetInputStreamAt(offset))
            using (var dataReader = new DataReader(inputStream))
            {
                dataReader.UnicodeEncoding = UnicodeEncoding.Utf8;
                dataReader.ByteOrder = ByteOrder.LittleEndian;

                await dataReader.LoadAsync((uint)_sourceStream.Size - (uint)offset);

                _payloadBuilder.SetPartAddingStrategy(infoAddingStrategy);
                _payloadBuilder.AddPart(dataReader);
            }
        }

        private static async Task ReadMapImagesAsync()
        {
            var itemInfo = GetItemInfo(ItemType.Image);

            if (itemInfo == null)
                return;

            var imageAddingStrategy = new MapFilePayloadImageAddingStrategy();

            var offset = (ulong)(_header.Size + _itemsOffsets[itemInfo.StartIndex]);

            using (var inputStream = _sourceStream.GetInputStreamAt(offset))
            using (var dataReader = new DataReader(inputStream))
            {
                dataReader.UnicodeEncoding = UnicodeEncoding.Utf8;
                dataReader.ByteOrder = ByteOrder.LittleEndian;

                await dataReader.LoadAsync((uint)_sourceStream.Size - (uint)offset);

                _payloadBuilder.SetPartAddingStrategy(imageAddingStrategy);
                    
                for (int i = 0; i < itemInfo.Number; i++)
                {
                    // Skip type_and_id and size
                    dataReader.ReadInt32();
                    dataReader.ReadInt32();

                    _payloadBuilder.AddPart(dataReader);
                }
            }
        }

        private static async Task ReadMapLayersAsync()
        {
            var itemInfo = GetItemInfo(ItemType.Layer);

            if (itemInfo == null)
                return;

            var tilesLayerAddingStrategy = new MapFilePayloadTilesLayerAddingStrategy();
            var quadsLayerAddingStrategy = new MapFilePayloadQuadsLayerAddingStrategy();

            for (int i = 0; i < itemInfo.Number; i++)
            {
                var offset = (ulong)(_header.Size + _itemsOffsets[itemInfo.StartIndex + i]);

                using (var inputStreamVersion = _sourceStream.GetInputStreamAt(offset))
                using (var dataReaderVersion = new DataReader(inputStreamVersion))
                using (var inputStream = _sourceStream.GetInputStreamAt(offset))
                using (var dataReader = new DataReader(inputStream))
                {
                    dataReader.UnicodeEncoding = dataReaderVersion.UnicodeEncoding = UnicodeEncoding.Utf8;
                    dataReader.ByteOrder = dataReaderVersion.ByteOrder = ByteOrder.LittleEndian;

                    var itemSize = (uint)GetItemSize(itemInfo.StartIndex + i);

                    await dataReaderVersion.LoadAsync(itemSize);

                    // Skip type_and_id and size
                    dataReaderVersion.ReadInt32();
                    dataReaderVersion.ReadInt32();

                    // Read layer base info
                    var layerBaseVersion = dataReaderVersion.ReadInt32();
                    var layerBaseType = dataReaderVersion.ReadInt32();
                    var layerBaseFlags = dataReaderVersion.ReadInt32();

                    // Read layer version
                    var itemVersion = dataReaderVersion.ReadInt32();

                    // Return back to layer start, to read the base layer 
                    await dataReader.LoadAsync(itemSize);

                    // Skip type_and_id and size
                    dataReader.ReadInt32();
                    dataReader.ReadInt32();

                    if ((LayerType)layerBaseType == LayerType.Tiles)
                    {
                        _payloadBuilder.SetPartAddingStrategy(tilesLayerAddingStrategy);
                        _payloadBuilder.AddPart(dataReader, itemVersion);
                    }
                    else if ((LayerType)layerBaseType == LayerType.Quads)
                    {
                        _payloadBuilder.SetPartAddingStrategy(quadsLayerAddingStrategy);
                        _payloadBuilder.AddPart(dataReader, itemVersion);
                    }
                }
            }
        }

        private static async Task ReadMapGroupsAsync()
        {
            var itemInfo = GetItemInfo(ItemType.Group);

            if (itemInfo == null)
                return;

            var groupAddingStrategy = new MapFilePayloadGroupAddingStrategy();

            var offset = (ulong)(_header.Size + _itemsOffsets[itemInfo.StartIndex]);

            using (var inputStream = _sourceStream.GetInputStreamAt(offset))
            using (var dataReader = new DataReader(inputStream))
            {
                dataReader.UnicodeEncoding = UnicodeEncoding.Utf8;
                dataReader.ByteOrder = ByteOrder.LittleEndian;

                await dataReader.LoadAsync((uint)_sourceStream.Size - (uint)offset);

                _payloadBuilder.SetPartAddingStrategy(groupAddingStrategy);

                for (int i = 0; i < itemInfo.Number; i++)
                {
                    // Skip type_and_id and size
                    dataReader.ReadInt32();
                    dataReader.ReadInt32();

                    _payloadBuilder.AddPart(dataReader);
                }
            }
        }

        private static async Task ReadMapEnvelopesAndPointsAsync()
        {
            var envelopeItemInfo = GetItemInfo(ItemType.Envelope);
            var envelopePointItemInfo = GetItemInfo(ItemType.EnvelopePoints);

            if (envelopeItemInfo == null || envelopePointItemInfo == null)
                return;

            var envelopeAddingStrategy = new MapFilePayloadEnvelopeAddingStrategy();
            var envelopePointAddingStrategy = new MapFilePayloadEnvelopePointAddingStrategy();

            var offsetEnvelope = (ulong)(_header.Size + _itemsOffsets[envelopeItemInfo.StartIndex]);
            var offsetEnvelopePoints = (ulong)(_header.Size + _itemsOffsets[envelopePointItemInfo.StartIndex]);

            // Loading of envelope points works differently because of the incorrect
            // addition of an array of points as a single element in the map items.

            // There is no way to know the data size of an individual point,
            // since Type_and_id and Size are specified once for the entire point array,
            // and the point version is selected depending on the envelope version to which the point belongs.

            using (var inputStreamEnvelope = _sourceStream.GetInputStreamAt(offsetEnvelope))
            using (var dataReaderEnvelope = new DataReader(inputStreamEnvelope))
            using (var inputStreamEnvelopePoints = _sourceStream.GetInputStreamAt(offsetEnvelopePoints))
            using (var dataReaderEnvelopePoints = new DataReader(inputStreamEnvelopePoints))
            {
                dataReaderEnvelope.UnicodeEncoding = dataReaderEnvelopePoints.UnicodeEncoding = UnicodeEncoding.Utf8;
                dataReaderEnvelope.ByteOrder = dataReaderEnvelopePoints.ByteOrder = ByteOrder.LittleEndian;

                await dataReaderEnvelopePoints.LoadAsync((uint)_sourceStream.Size - (uint)offsetEnvelopePoints);

                // Skip type_and_id and size
                dataReaderEnvelopePoints.ReadInt32();
                dataReaderEnvelopePoints.ReadInt32();

                for (int i = 0; i < envelopeItemInfo.Number; i++)
                {
                    await dataReaderEnvelope.LoadAsync((uint)GetItemSize(envelopeItemInfo.StartIndex + i));

                    // Skip type_and_id and size
                    dataReaderEnvelope.ReadInt32();
                    dataReaderEnvelope.ReadInt32();

                    _payloadBuilder.SetPartAddingStrategy(envelopeAddingStrategy);
                    var envelopeDTO = (MapEnvelopeDTO_v1)_payloadBuilder.AddPart(dataReaderEnvelope);

                    _payloadBuilder.SetPartAddingStrategy(envelopePointAddingStrategy);

                    int pointsVersion = envelopeDTO.version >= 3 ? 2 : 1; // The problem is due to a lack of version in CEnvPoint from the original TeeWorlds client

                    for (int j = 0; j < envelopeDTO.pointsNumber; j++)
                    {
                        _payloadBuilder.AddPart(dataReaderEnvelopePoints, pointsVersion);
                    }
                }
            }
        }

        private static InputItemInfo GetItemInfo(ItemType type)
            => _itemsInfo.FirstOrDefault(x => x.Type == type);

        private static int GetItemSize(int index)
        {
            if (index == _header.ItemsNumber - 1)
                return _header.ItemsSequenceSize - _itemsOffsets[index];

            return _itemsOffsets[index + 1] - _itemsOffsets[index];
        }

        private static int GetDataSize(int index)
        {
            if (index == _header.DataNumber - 1)
                return _header.DataSequenceSize - _dataOffsets[index];

            else if (index < _header.DataNumber - 1)
                return _dataOffsets[index + 1] - _dataOffsets[index];

            return 0;
        }
    }
}