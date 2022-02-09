using System.Runtime.InteropServices;

namespace Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapQuadsLayerDTO_v1 : MapLayerDTO
    {
        [MarshalAs(UnmanagedType.I4)]
        public int version;

        [MarshalAs(UnmanagedType.I4)]
        public int quadsNumber;

        [MarshalAs(UnmanagedType.I4)]
        public int quadsDataIndex;

        [MarshalAs(UnmanagedType.I4)]
        public int imageId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapQuadsLayerDTO : MapQuadsLayerDTO_v1
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] nameArray;
    }
}
