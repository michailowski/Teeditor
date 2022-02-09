using System.Runtime.InteropServices;

namespace Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapTilesLayerDTO_v1 : MapLayerDTO
    {
        [MarshalAs(UnmanagedType.I4)]
        public int version;

        [MarshalAs(UnmanagedType.I4)]
        public int width;

        [MarshalAs(UnmanagedType.I4)]
        public int height;

        [MarshalAs(UnmanagedType.I4)]
        public int flags;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] color;

        [MarshalAs(UnmanagedType.I4)]
        public int colorEnvelopeId;

        [MarshalAs(UnmanagedType.I4)]
        public int colorEnvelopeOffset;

        [MarshalAs(UnmanagedType.I4)]
        public int imageId;

        [MarshalAs(UnmanagedType.I4)]
        public int tilesDataIndex;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapTilesLayerDTO_v2 : MapTilesLayerDTO_v1
    {
        // The original source code does not contain data about this version
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapTilesLayerDTO_v3 : MapTilesLayerDTO_v2
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] nameArray;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapTilesLayerDTO : MapTilesLayerDTO_v3
    {
        // The original source code does not contain data about this version
    }
}
