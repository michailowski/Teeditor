using System.Runtime.InteropServices;

namespace Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapImageDTO_v1 : IMapItemDTO
    {
        [MarshalAs(UnmanagedType.I4)]
        public int version;

        [MarshalAs(UnmanagedType.I4)]
        public int width;

        [MarshalAs(UnmanagedType.I4)]
        public int height;

        [MarshalAs(UnmanagedType.I4)]
        public int isExternal;

        [MarshalAs(UnmanagedType.I4)]
        public int nameDataIndex;

        [MarshalAs(UnmanagedType.I4)]
        public int imageDataIndex;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapImageDTO : MapImageDTO_v1
    {
        [MarshalAs(UnmanagedType.I4)]
        public int format;
    }
}
