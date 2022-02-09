using System;
using Teeditor.Common.Models.Bindable;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Windows.Foundation;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic
{
    internal class TilesLayerSelection : BindableBase, ILayerSelection
    {
        private MapTilesLayer _sourceLayer;
        private bool _isEmpty = true;
        private bool _isTransformationAllowed = false;

        public MapTile[] Tiles { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public bool IsEmpty
        {
            get => _isEmpty;
            private set => Set(ref _isEmpty, value);
        }

        public bool IsTransformationAllowed
        {
            get => _isTransformationAllowed;
            private set => Set(ref _isTransformationAllowed, value);
        }

        public void Select(MapTilesLayer layer, Rect sourceRect)
        {
            _sourceLayer = layer;
            Width = (int)sourceRect.Width;
            Height = (int)sourceRect.Height;

            var sourceX = (int)sourceRect.Left;
            var sourceY = (int)sourceRect.Top;

            Tiles = new MapTile[Width * Height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var srcTile = _sourceLayer.Tiles[sourceX + x + (sourceY + y) * _sourceLayer.Width];
                    var dstTile = new MapTile(srcTile.Index, srcTile.Flags, 0, srcTile.Reserved);

                    Tiles[x + y * Width] = dstTile;
                }
            }

            IsEmpty = false;
            IsTransformationAllowed = true;
        }

        public void FlipHorizontal()
        {
            if (IsTransformationAllowed == false)
                return;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width / 2; x++)
                {
                    var tmp = Tiles[x + y * Width];
                    Tiles[x + y * Width] = Tiles[Width - 1 - x + y * Width];
                    Tiles[Width - 1 - x + y * Width] = tmp;
                }
            }

            if (_sourceLayer.IsGameLayer == false)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        Tiles[x + y * Width].FlipHorizontal();
                    }
                }
            }
        }

        public void FlipVertical()
        {
            if (IsEmpty)
                return;

            for (int y = 0; y < Height / 2; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var tmpTiles = Tiles[x + y * Width];
                    Tiles[x + y * Width] = Tiles[x + (Height - 1 - y) * Width];
                    Tiles[x + (Height - 1 - y) * Width] = tmpTiles;
                }
            }

            if (_sourceLayer.IsGameLayer == false)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        Tiles[x + y * Width].FlipVertical();
                    }
                }
            }
        }

        public void Rotate(float amount)
        {
            if (IsTransformationAllowed == false)
                return;

            int rotation = (int)(360.0f * amount / (Math.PI * 2)) / 90 % 4; // 0=0°, 1=90°, 2=180°, 3=270°

            if (rotation < 0)
                rotation += 4;

            // 90° rotation
            if (rotation == 1 || rotation == 3)
            {
                var tmpTiles = new MapTile[Width * Height];
                Array.Copy(Tiles, tmpTiles, Tiles.Length);

                int index = 0;

                for (int x = 0; x < Width; ++x)
                {
                    for (int y = Height - 1; y >= 0; --y, ++index)
                    {
                        Tiles[index] = tmpTiles[y * Width + x];

                        if (_sourceLayer.IsGameLayer == false)
                        {
                            Tiles[index].Rotate(TileDirectionOfRotation.Right);
                        }
                    }
                }

                int tmp = Width;
                Width = Height;
                Height = tmp;
            }

            // 180° rotation
            if (rotation == 2 || rotation == 3)
            {
                FlipVertical();
                FlipHorizontal();
            }
        }

        public void Clear()
        {
            Tiles = null;
            Width = 0;
            Height = 0;
            IsEmpty = true;
            IsTransformationAllowed = false;
        }
    }
}
