using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Windows.Storage.Streams;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload
{
    internal abstract class MapFilePayloadPartAddingStrategy
    {
        protected MapFilePayload _payload;

        public void SetData(MapFilePayload payload)
            => _payload = payload;

        public virtual IMapItemDTO Add(MapItem mapItem)
            => null;

        public virtual Task<IMapItemDTO> AddAsync(MapItem mapItem)
            => Task.FromResult((IMapItemDTO)null);

        public abstract void Add(IMapItemDTO mapItemDTO);

        public IMapItemDTO Add(DataReader dataReader)
        {
            var mapItemDTO = Read(dataReader);
            Add(mapItemDTO);

            return mapItemDTO;
        }

        public IMapItemDTO Add(DataReader dataReader, int version)
        {
            var mapItemDTO = Read(dataReader, version, false);
            Add(mapItemDTO);

            return mapItemDTO;
        }

        private IMapItemDTO Read(DataReader dataReader)
        {
            var version = dataReader.ReadInt32();

            return Read(dataReader, version, true);
        }

        private IMapItemDTO Read(DataReader dataReader, int version, bool replace)
        {
            var type = GetItemDtoType(version);
            var size = Marshal.SizeOf(type);
            var data = new byte[size];

            if (replace)
            {
                var dataWithoutVersion = new byte[size - sizeof(int)];
                var versionData = BitConverter.GetBytes(version);

                dataReader.ReadBytes(dataWithoutVersion);
                versionData.CopyTo(data, 0);
                dataWithoutVersion.CopyTo(data, versionData.Length);
            }
            else
            {
                dataReader.ReadBytes(data);
            }

            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(data, 0, ptr, size);

            var dto = (IMapItemDTO)Marshal.PtrToStructure(ptr, GetItemDtoType(version));

            Marshal.FreeHGlobal(ptr);

            return dto;
        }

        protected abstract Type GetItemDtoType(int version);
    }
}
