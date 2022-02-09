using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO
{
    internal class InputItemInfo
    {
        public ItemType Type { get; set; }

        public int StartIndex { get; set; }

        public int Number { get; set; }

        public InputItemInfo(ItemType type, int startIndex, int number)
        {
            Type = type;
            StartIndex = startIndex;
            Number = number;
        }
    }
}
