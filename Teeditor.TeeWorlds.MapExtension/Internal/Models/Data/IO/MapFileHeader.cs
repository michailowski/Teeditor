namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO
{
    internal class MapFileHeader
    {
        private int _size = 0;
        public int Version { get; set; }
        public int Size
        {
            get => 36 + ItemTypesNumber * 12 + (ItemsNumber + 2 * DataNumber) * sizeof(int);
            set => _size = value;
        }
        public int SwapLength { get; set; }
        public int ItemTypesNumber { get; set; }
        public int ItemsNumber { get; set; }
        public int DataNumber { get; set; }
        public int ItemsSequenceSize { get; set; }
        public int DataSequenceSize { get; set; }
        public bool IsValidVersion => Version == 3 || Version == 4;
    }
}
