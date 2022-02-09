using System.Runtime.InteropServices;

namespace Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapInfoDTO : IMapItemDTO
    {
        [MarshalAs(UnmanagedType.I4)]
        public int version;

        [MarshalAs(UnmanagedType.I4)]
        public int authorDataIndex;

        [MarshalAs(UnmanagedType.I4)]
        public int mapVersionDataIndex;

        [MarshalAs(UnmanagedType.I4)]
        public int creditsDataIndex;

        [MarshalAs(UnmanagedType.I4)]
        public int licenseDataIndex;
    }
}
