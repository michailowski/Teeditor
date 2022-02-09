using System;
using System.Threading.Tasks;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Windows.Storage;
using Windows.Storage.Streams;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using System.Runtime.InteropServices;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload.Strategies;
using System.Collections.Generic;
using System.Linq;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO
{
    internal static class MapFileWriter
    {
        private static IRandomAccessStream _destinationStream;

        private static MapFilePayloadBuilder _payloadBuilder;
        private static MapFilePayload _payload;
        private static Dictionary<ItemType, List<int>> _itemTypes;

        private const int DatafileItemSize = sizeof(int) * 2; // Type_and_id and Size

        public static async Task WriteAsync(Map map, StorageFile storageFile)
        {
            _destinationStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite);

            _payloadBuilder = new MapFilePayloadBuilder();
            _itemTypes = new Dictionary<ItemType, List<int>>();

            await Prepare(map);
            await Finish();

            _destinationStream.Dispose();
            _destinationStream = null;
        }

        #region Prepare Methods

        private static async Task Prepare(Map map)
        {
            PrepareMapVersion();
            await PrepareMapInfo(map);
            await PrepareMapImages(map);
            await PrepareMapGroups (map);
            PrepareMapEnvelopes(map);

            _payload = _payloadBuilder.GetResult();
        }

        private static void PrepareMapVersion()
        {
            var addingStrategy = new MapFilePayloadVersionAddingStrategy();
            _payloadBuilder.SetPartAddingStrategy(addingStrategy);

            var dto = new MapVersionDTO();
            dto.version = (int)ItemCurrentVersion.Map;

            _payloadBuilder.AddPart(dto);

            _itemTypes.Add(ItemType.Version, new List<int>() { Marshal.SizeOf(dto) });
        }

        private static async Task PrepareMapInfo(Map map)
        {
            var addingStrategy = new MapFilePayloadInfoAddingStrategy();
            _payloadBuilder.SetPartAddingStrategy(addingStrategy);

            var dto = await _payloadBuilder.AddPartAsync(map.Info);

            _itemTypes.Add(ItemType.Info, new List<int>() { Marshal.SizeOf(dto) });
        }

        private static async Task PrepareMapImages(Map map)
        {
            var addingStrategy = new MapFilePayloadImageAddingStrategy();
            _payloadBuilder.SetPartAddingStrategy(addingStrategy);

            if (map.ImagesContainer.Items.Count > 0)
                _itemTypes.Add(ItemType.Image, new List<int>());

            foreach (var image in map.ImagesContainer.Items)
            {
                var dto = await _payloadBuilder.AddPartAsync(image);
                _itemTypes[ItemType.Image].Add(Marshal.SizeOf(dto));
            }
        }

        private static async Task PrepareMapGroups(Map map)
        {
            var groupAddingStrategy = new MapFilePayloadGroupAddingStrategy();
            var tilesLayerAddingStrategy = new MapFilePayloadTilesLayerAddingStrategy();
            var quadsLayerAddingStrategy = new MapFilePayloadQuadsLayerAddingStrategy();

            if (map.GroupedLayersContainer.Groups.Count > 0)
                _itemTypes.Add(ItemType.Group, new List<int>());

            foreach (var group in map.GroupedLayersContainer.Groups)
            {
                _payloadBuilder.SetPartAddingStrategy(groupAddingStrategy);
                var dto = _payloadBuilder.AddPart(group);

                _itemTypes[ItemType.Group].Add(Marshal.SizeOf(dto));

                if (group.Layers.Count > 0)
                    _itemTypes.TryAdd(ItemType.Layer, new List<int>());

                foreach (var layer in group.Layers)
                {
                    if (layer is MapTilesLayer)
                    {
                        _payloadBuilder.SetPartAddingStrategy(tilesLayerAddingStrategy);
                        var tilesLayerDTO = await _payloadBuilder.AddPartAsync(layer);
                        _itemTypes[ItemType.Layer].Add(Marshal.SizeOf(tilesLayerDTO));
                    }
                    else if (layer is MapQuadsLayer)
                    {
                        _payloadBuilder.SetPartAddingStrategy(quadsLayerAddingStrategy);
                        var quadsLayerDTO = await _payloadBuilder.AddPartAsync(layer);
                        _itemTypes[ItemType.Layer].Add(Marshal.SizeOf(quadsLayerDTO));
                    }
                }
            }
        }

        private static void PrepareMapEnvelopes(Map map)
        {
            var envelopeAddingStrategy = new MapFilePayloadEnvelopeAddingStrategy();
            var envelopePointAddingStrategy = new MapFilePayloadEnvelopePointAddingStrategy();

            if (map.EnvelopesContainer.Items.Count > 0)
                _itemTypes.Add(ItemType.Envelope, new List<int>());

            // I don't know why this item in the original client is necessarily added
            _itemTypes.TryAdd(ItemType.EnvelopePoints, new List<int>() { 0 });

            foreach (var envelope in map.EnvelopesContainer.Items)
            {
                _payloadBuilder.SetPartAddingStrategy(envelopeAddingStrategy);
                var dto = _payloadBuilder.AddPart(envelope);

                _itemTypes[ItemType.Envelope].Add(Marshal.SizeOf(dto));

                _payloadBuilder.SetPartAddingStrategy(envelopePointAddingStrategy);

                //if (envelope.Points.Count > 0)
                //    _itemTypes.TryAdd(ItemType.EnvelopePoints, new List<int>() { 0 });

                // That is bad, very bad. It would be better to count one item for each point.
                foreach (var envelopePoint in envelope.Points)
                {
                    var pointDTO = _payloadBuilder.AddPart(envelopePoint);
                    _itemTypes[ItemType.EnvelopePoints][0] += Marshal.SizeOf(pointDTO);
                }
            }
        }

        #endregion

        #region Finish Methods

        private static async Task Finish()
        {
            using (var outputStream = _destinationStream.GetOutputStreamAt(0))
            using (var dw = new DataWriter(outputStream))
            {
                dw.UnicodeEncoding = UnicodeEncoding.Utf8;
                dw.ByteOrder = ByteOrder.LittleEndian;

                WriteSignature(dw);
                WriteHeader(dw);
                WriteItemsInfo(dw);
                WriteItemsOffsets(dw);
                WriteDataOffsets(dw);
                WriteDataUncompressedSizes(dw);
                WriteItems(dw);
                WriteData(dw);

                await dw.StoreAsync();
                await outputStream.FlushAsync();
            }
        }

        private static void WriteSignature(DataWriter dw)
        {
            string fileSignature = "DATA";
            dw.WriteString(fileSignature);
        }

        private static void WriteHeader(DataWriter dw)
        {
            int itemsSequenceSize = 0;
            int itemsNumber = 0;
            int dataSequenceSize = 0;
            int datasNumber = 0;

            foreach (var itemType in _itemTypes)
            {
                foreach (var itemSize in itemType.Value)
                {
                    itemsSequenceSize += itemSize + DatafileItemSize;
                }

                itemsNumber += itemType.Value.Count;
            }

            for (int i = 0; i < _payload.Data.CompressedDataNumber; i++)
            {
                var hasData = _payload.Data.TryGetCompressed(i, out var compressedData, out var compressedDataSize, out var decompressedDataSize);

                if (hasData == false)
                    continue;

                dataSequenceSize += compressedDataSize;
                datasNumber++;
            }

            var typesInfoSize = _itemTypes.Count * sizeof(int) * 3;

            var signatureSize = 4;
            var headerSize = signatureSize + sizeof(int) * 8;

            var itemsOffsetsSequenceSize = itemsNumber * sizeof(int);
            var dataOffsetsSequenceSize = datasNumber * sizeof(int);
            var uncompressedDataOffsetsSequenceSize = datasNumber * sizeof(int);

            var offsetsSequenceSize = itemsOffsetsSequenceSize + dataOffsetsSequenceSize + uncompressedDataOffsetsSequenceSize;
            var fileSize = headerSize + typesInfoSize + offsetsSequenceSize + itemsSequenceSize + dataSequenceSize;
            var swapSize = fileSize - dataSequenceSize;

            dw.WriteInt32(4); // Header version
            dw.WriteInt32(fileSize - 16);
            dw.WriteInt32(swapSize - 16);
            dw.WriteInt32(_itemTypes.Count);
            dw.WriteInt32(itemsNumber);
            dw.WriteInt32(datasNumber);
            dw.WriteInt32(itemsSequenceSize);
            dw.WriteInt32(dataSequenceSize);
        }

        private static void WriteItemsInfo(DataWriter dw)
        {
            int itemsNumber = 0;

            foreach (var itemType in _itemTypes.OrderBy(x => x.Key))
            {
                dw.WriteInt32((int)itemType.Key);
                dw.WriteInt32(itemsNumber);
                dw.WriteInt32(itemType.Value.Count);

                itemsNumber += itemType.Value.Count;
            }
        }

        private static void WriteItemsOffsets(DataWriter dw)
        {
            int offset = 0;

            foreach (var itemType in _itemTypes.OrderBy(x => x.Key))
            {
                foreach (var itemSize in itemType.Value)
                {
                    dw.WriteInt32(offset);

                    offset += itemSize + DatafileItemSize;
                }
            }
        }

        private static void WriteDataOffsets(DataWriter dw)
        {
            int offset = 0;

            for (int i = 0; i < _payload.Data.CompressedDataNumber; i++)
            {
                var hasData = _payload.Data.TryGetCompressed(i, out var compressedData, out var compressedDataSize, out var decompressedDataSize);

                if (hasData == false)
                    continue;

                dw.WriteInt32(offset);

                offset += compressedDataSize;
            }
        }

        private static void WriteDataUncompressedSizes(DataWriter dw)
        {
            for (int i = 0; i < _payload.Data.CompressedDataNumber; i++)
            {
                var hasData = _payload.Data.TryGetCompressed(i, out var compressedData, out var compressedDataSize, out var decompressedDataSize);

                if (hasData == false)
                    continue;

                dw.WriteInt32(decompressedDataSize);
            }
        }

        private static void WriteItems(DataWriter dw)
        {
            WriteItem(dw, (int)ItemType.Version << 16 | 0, _payload.Items.VersionDTO);

            WriteItem(dw, (int)ItemType.Info << 16 | 0, _payload.Items.InfoDTO);

            for (int i = 0; i < _payload.Items.ImageDTOs.Count; i++)
            {
                WriteItem(dw, (int)ItemType.Image << 16 | i, _payload.Items.ImageDTOs[i]);
            }

            for (int i = 0; i < _payload.Items.EnvelopeDTOs.Count; i++)
            {
                WriteItem(dw, (int)ItemType.Envelope << 16 | i, _payload.Items.EnvelopeDTOs[i]);
            }

            for (int i = 0; i < _payload.Items.GroupDTOs.Count; i++)
            {
                WriteItem(dw, (int)ItemType.Group << 16 | i, _payload.Items.GroupDTOs[i]);
            }

            for (int i = 0; i < _payload.Items.LayerDTOs.Count; i++)
            {
                WriteItem(dw, (int)ItemType.Layer << 16 | i, _payload.Items.LayerDTOs[i]);
            }

            // The consequence of envelope points problem
            dw.WriteInt32((int)ItemType.EnvelopePoints << 16 | 0);
            dw.WriteInt32(Marshal.SizeOf(typeof(MapEnvelopeDTO)) * _payload.Items.EnvelopePointDTOs.Count);
            for (int i = 0; i < _payload.Items.EnvelopePointDTOs.Count; i++)
            {
                dw.WriteBytes(GetBytes(_payload.Items.EnvelopePointDTOs[i]));
            }
        }

        private static void WriteItem(DataWriter dw, int typeAndId, IMapItemDTO dto)
        {
            dw.WriteInt32(typeAndId);
            dw.WriteInt32(Marshal.SizeOf(dto));
            dw.WriteBytes(GetBytes(dto));
        }

        private static void WriteData(DataWriter dw)
        {
            for (int i = 0; i < _payload.Data.CompressedDataNumber; i++)
            {
                var hasData = _payload.Data.TryGetCompressed(i, out var compressedData, out var compressedDataSize, out var decompressedDataSize);

                if (hasData == false)
                    continue;

                dw.WriteBytes(compressedData);
            }
        }

        private static byte[] GetBytes(IMapItemDTO dto)
        {
            int size = Marshal.SizeOf(dto);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(dto, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        #endregion
    }
}