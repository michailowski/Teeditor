using System.Runtime.InteropServices;

namespace Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapEnvelopeDTO_v1 : IMapItemDTO
    {
        [MarshalAs(UnmanagedType.I4)]
        public int version;

        [MarshalAs(UnmanagedType.I4)]
        public int channelsNumber;

        [MarshalAs(UnmanagedType.I4)]
        public int startPointIndex;

        [MarshalAs(UnmanagedType.I4)]
        public int pointsNumber;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public int[] nameArray;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapEnvelopeDTO_v2 : MapEnvelopeDTO_v1
    {
        [MarshalAs(UnmanagedType.I4)]
        public int isSynchronized;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapEnvelopeDTO : MapEnvelopeDTO_v2
    {
        // bezier curve support
    }
}
