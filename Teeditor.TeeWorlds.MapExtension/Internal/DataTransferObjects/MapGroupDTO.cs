using System.Runtime.InteropServices;

namespace Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapGroupDTO_v1 : IMapItemDTO
    {
        [MarshalAs(UnmanagedType.I4)]
        public int version;

        [MarshalAs(UnmanagedType.I4)]
        public int offsetX;

        [MarshalAs(UnmanagedType.I4)]
        public int offsetY;

        [MarshalAs(UnmanagedType.I4)]
        public int parallaxX;

        [MarshalAs(UnmanagedType.I4)]
        public int parallaxY;

        [MarshalAs(UnmanagedType.I4)]
        public int startLayerIndex;

        [MarshalAs(UnmanagedType.I4)]
        public int layersNumber;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapGroupDTO_v2 : MapGroupDTO_v1
    {
        [MarshalAs(UnmanagedType.I4)]
        public int useClipping;

        [MarshalAs(UnmanagedType.I4)]
        public int clipX;

        [MarshalAs(UnmanagedType.I4)]
        public int clipY;

        [MarshalAs(UnmanagedType.I4)]
        public int clipW;

        [MarshalAs(UnmanagedType.I4)]
        public int clipH;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapGroupDTO : MapGroupDTO_v2
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] nameArray;
    }
}
