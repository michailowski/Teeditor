using System.Linq;
using System.Text;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Utilities
{
    internal static class StringUtilities
    {
        public static int[] StrToInts(this string input, int number)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            var array = new int[number];
            var index = 0;

            for (var i = 0; i < array.Length; i++)
            {
                var buffer = new[] { 0, 0, 0, 0 };

                for (var c = 0; c < buffer.Length && index < bytes.Length; c++, index++)
                {
                    buffer[c] = (sbyte)bytes[index];
                }

                array[i] = buffer[0] + 128 << 24 | buffer[1] + 128 << 16 |
                           buffer[2] + 128 << 08 | buffer[3] + 128 << 00;
            }

            array[array.Length - 1] = (int)(array[array.Length - 1] & 0xffff_ff00);
            return array;
        }

        public static string IntsToStr(this int[] array)
        {
            byte[] buffer = new byte[array.Length * sizeof(int)];
            int charNumber = 4;
            var length = 0;

            for (var i = 0; i < array.Length; i++)
            {
                for (var j = charNumber - 1; j >= 0; j--)
                {
                    buffer[i * charNumber + j] = (byte)((array[i] >> (charNumber - j - 1) * 8 & 0xFF) - 128);

                    if (buffer[i * charNumber + j] >= 32)
                    {
                        length++;
                    }
                }
            }

            return length == 0 ? string.Empty : Encoding.UTF8.GetString(buffer.Take(length - 1).ToArray());
        }
    }
}
