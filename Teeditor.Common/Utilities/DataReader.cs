using Windows.Storage.Streams;

namespace Teeditor.Common.Utilities
{
    public static class DataReaderExtensions
    {
        public static int[] ReadInt32Array(this DataReader reader, int count)
        {
            int[] data = new int[count];

            for (int i = 0; i < count; i++)
                data[i] = reader.ReadInt32();

            return data;
        }

        public static void WriteInt32Array(this DataWriter writer, int[] data)
        {
            for (int i = 0; i < data.Length; i++)
                writer.WriteInt32(data[i]);
        }
    }
}
