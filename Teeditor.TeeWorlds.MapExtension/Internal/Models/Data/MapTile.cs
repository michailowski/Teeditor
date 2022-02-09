using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data
{
    internal struct MapTile
    {
        public byte Index { get; set; }
        public byte Flags { get; set; }
        public byte Skip { get; set; }
        public byte Reserved { get; set; }

        public bool IsRotated => (Flags & (byte)TileProperties.Rotate) != 0;
        public bool IsVerticallyFlipped => (Flags & (byte)TileProperties.VerticalFlip) != 0;
        public bool IsHorizontallyFlipped => (Flags & (byte)TileProperties.HorizontalFlip) != 0;

        public MapTile(byte index = 0, byte flags = 0, byte skip = 0, byte reserved = 0)
        {
            Index = index;
            Flags = flags;
            Skip = skip;
            Reserved = reserved;
        }

        public void FlipHorizontal()
        {
            if (IsRotated)
            {
                Flags ^= (byte)TileProperties.HorizontalFlip;
            }
            else
            {
                Flags ^= (byte)TileProperties.VerticalFlip;
            }
        }

        public void FlipVertical()
        {
            if (IsRotated)
            {
                Flags ^= (byte)TileProperties.VerticalFlip;
            }
            else
            {
                Flags ^= (byte)TileProperties.HorizontalFlip;
            }
        }

        public void Rotate(TileDirectionOfRotation direction)
        {
            if (direction == TileDirectionOfRotation.Right)
            {
                if (IsRotated)
                    Flags ^= (byte)TileProperties.HorizontalFlip | (byte)TileProperties.VerticalFlip;

                Flags ^= (byte)TileProperties.Rotate;
            }
            else if (direction == TileDirectionOfRotation.Left)
            {
                if (IsRotated)
                    Flags ^= (byte)TileProperties.HorizontalFlip | (byte)TileProperties.VerticalFlip;

                Flags ^= (byte)TileProperties.Rotate;

                FlipVertical();
                FlipHorizontal();
            }
        }

        public override bool Equals(object o)
        {
            return false;
            //if (o == null)
            //    return false;

            //var other = o as MapTileViewModel;

            //return other != null &&
            //       this.Index == other.Index &&
            //       this.Flags == other.Flags &&
            //       this.Skip == other.Skip &&
            //       this.Reserved == other.Reserved;
        }

        public bool Equals(MapTile other)
        {
            return Index == other.Index &&
                   Flags == other.Flags &&
                   Skip == other.Skip &&
                   Reserved == other.Reserved;
        }

        // TODO: hashcode
        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(MapTile t1, MapTile t2) => t1.Equals(t2);

        public static bool operator !=(MapTile t1, MapTile t2) => !t1.Equals(t2);
    }
}
