using System.Collections.Generic;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic
{
    internal static class TilesPacker
    {
        public static byte[] Pack(MapTile[] tiles, int width, int height)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tiles[x + y * width].Flags &= (byte)TileProperties.VerticalFlip | (byte)TileProperties.HorizontalFlip | (byte)TileProperties.Rotate;

                    if (tiles[x + y * width].Index == 0)
                    {
                        tiles[x + y * width].Flags = 0;
                    }
                }
            }

            byte maxSkip = (1 << 8) - 1;
            int tilesNumber = 0;
            int maxSkipHitNumber = -1;

            MapTile currentTile = new MapTile();
            currentTile.Skip = maxSkip;

            for (int i = 0; i < width * height; i++)
            {
                if (currentTile.Skip == maxSkip)
                {
                    currentTile = tiles[i];
                    currentTile.Skip = 0;

                    tilesNumber++;
                    maxSkipHitNumber++;

                    continue;
                }
                else if (tiles[i].Index != currentTile.Index || tiles[i].Flags != currentTile.Flags)
                {
                    currentTile = tiles[i];
                    currentTile.Skip = 0;

                    tilesNumber++;

                    continue;
                }

                currentTile.Skip++;
            }

            var saveTiles = new MapTile[tilesNumber];
            int writtenSaveTilesNumber = 0;
            currentTile.Skip = maxSkip;

            for (int i = 0; i < width * height + 1; i++)
            {
                if (i != width * height && currentTile.Skip != maxSkip && tiles[i].Index == currentTile.Index && tiles[i].Flags == currentTile.Flags)
                {
                    currentTile.Skip++;
                    continue;
                }

                if (i != 0)
                {
                    saveTiles[writtenSaveTilesNumber++] = currentTile;
                }

                if (i != width * height)
                {
                    currentTile = tiles[i];
                    currentTile.Skip = 0;
                }
            }

            var data = new byte[4 * tilesNumber];

            for (int i = 0; i < saveTiles.Length; i++)
            {
                data[i * 4] = saveTiles[i].Index;
                data[i * 4 + 1] = saveTiles[i].Flags;
                data[i * 4 + 2] = saveTiles[i].Skip;
                data[i * 4 + 3] = saveTiles[i].Reserved;
            }

            return data;
        }

        public static MapTile[] Unpack(byte[] data, bool useSkip)
        {
            var list = new List<MapTile>();

            if (useSkip)
            {
                for (int i = 0; i < data.Length; i += 4)
                {
                    list.Add(new MapTile(data[i], data[i + 1], data[i + 2], data[i + 3]));

                    while (data[i + 2] > 0)
                    {
                        data[i + 2]--;
                        list.Add(new MapTile(data[i], data[i + 1], data[i + 2], data[i + 3]));
                    }
                }
            }
            else
            {
                for (int i = 0; i < data.Length; i += 4)
                {
                    list.Add(new MapTile(data[i], data[i + 1], data[i + 2], data[i + 3]));
                }
            }

            return list.ToArray();
        }
    }
}
