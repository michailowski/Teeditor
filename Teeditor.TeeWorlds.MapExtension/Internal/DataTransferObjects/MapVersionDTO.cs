using System.Runtime.InteropServices;

namespace Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapVersionDTO : IMapItemDTO
    {
        [MarshalAs(UnmanagedType.I4)]
        public int version;
    }
}
