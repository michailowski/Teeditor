using Ionic.Zlib;
using System.Threading.Tasks;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Utilities
{
    internal static class CompressionUtilities
    {
        public static async Task<byte[]> DecompressDataAsync(this byte[] data)
            => await Task.Run(() => ZlibStream.UncompressBuffer(data));

        public static async Task<byte[]> CompressDataAsync(this byte[] data)
            => await Task.Run(() => ZlibStream.CompressBuffer(data));
    }
}