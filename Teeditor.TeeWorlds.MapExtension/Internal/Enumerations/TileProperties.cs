using System;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Enumerations
{
    [Flags]
    internal enum TileProperties
    {
        None = 0,
        VerticalFlip = 1,
        HorizontalFlip = 2,
        Opaque = 4,
        Rotate = 8
    }
}
