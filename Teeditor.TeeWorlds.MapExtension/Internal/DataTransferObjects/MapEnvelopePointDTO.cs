using System.Runtime.InteropServices;

namespace Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapEnvelopePointDTO_v1 : IMapItemDTO
    {
        [MarshalAs(UnmanagedType.I4)]
        public int time;

        [MarshalAs(UnmanagedType.I4)]
        public int curveType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] values;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal class MapEnvelopePointDTO : MapEnvelopePointDTO_v1
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] inTangentdx;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] inTangentdy;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] outTangentdx;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] outTangentdy;
    }
}
