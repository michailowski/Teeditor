using System.Runtime.InteropServices;

namespace Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapLayerDTO : IMapItemDTO
    {
        [MarshalAs(UnmanagedType.I4)]
        public int baseVersion;

        [MarshalAs(UnmanagedType.I4)]
        public int baseType;

        [MarshalAs(UnmanagedType.I4)]
        public int baseFlags;
    }
}
