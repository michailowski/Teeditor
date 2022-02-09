using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager.Drawers;
using System.Threading.Tasks;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using System.ComponentModel;
using Teeditor.Common.Models.Commands;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data
{
    internal class MapTilesLayer : MapLayer
    {
        private string _name;
        private int _width;
        private int _height;
        private MapImage _image;
        private int[] _colorChannels = new int[] { 255, 255, 255, 255 };
        private MapEnvelope _colorEnvelope;
        private int _colorEnvelopeOffset;
        private Color _color = Colors.White;
        private bool _isGameLayer;
        private MapTile[] _tiles;
        
        private static TilesLayerDrawStrategy _drawStrategy;

        private Color _expectationOldColor;
        private bool _isColorExpectationStarted;

        [ModificationCommandLabelAttribute("Tiles layer name changed")]
        public override string Name
        {
            get => GetName();
            set => Set(ref _name, value, nameof(_name));
        }

        public int Width
        {
            get => _width;
            set => SetWidth(value);
        }

        public int Height
        {
            get => _height;
            set => SetHeight(value);
        }

        public int ImageId { get; set; } = -1;

        [ModificationCommandLabel("Tiles layer image changed")]
        public MapImage Image
        {
            get => _image;
            set => SetImage(value);
        }

        public int[] ColorChannels 
        { 
            get => _colorChannels;
            private set => _colorChannels = value; 
        }

        [ModificationCommandLabel("Tiles layer color changed")]
        public Color Color
        {
            get => _color;
            set => Set(ref _color, value);
        }

        public int ColorEnvelopeId { get; set; } = -1;

        [ModificationCommandLabel("Tiles layer color envelope changed")]
        public MapEnvelope ColorEnvelope
        {
            get => _colorEnvelope;
            set => SetColorEnvelope(value);
        }

        [ModificationCommandLabel("Tiles layer color envelope offset changed")]
        public int ColorEnvelopeOffset
        {
            get => _colorEnvelopeOffset;
            set => Set(ref _colorEnvelopeOffset, value, nameof(_colorEnvelopeOffset));
        }

        public MapTile[] Tiles => _tiles;

        public bool IsGameLayer => _isGameLayer;

        public CanvasCommandList Thumbnail { get; private set; }

        public bool HasThumbnail => Thumbnail != null;

        public bool IsThumbnailBeingDrawn { get; set; }

        public MapTilesLayer(int width, int height, bool isGameLayer, MapTile[] tiles)
        {
            _width = width;
            _height = height;
            _isGameLayer = isGameLayer;
            _tiles = tiles;

            PropertyChanged += MapTilesLayer_PropertyChanged;
        }

        public MapTilesLayer(int width, int height, bool isGameLayer = false)
        {
            _width = width;
            _height = height;
            _isGameLayer = isGameLayer;

            InitEmptyTiles();

            PropertyChanged += MapTilesLayer_PropertyChanged;
        }

        private void InitEmptyTiles()
        {
            _tiles = new MapTile[_width * _height];

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    _tiles[x + y * _width] = new MapTile(0, 0, 0, 0);
                }
            }
        }

        #region Get and Set Methods

        private string GetName()
        {
            if (IsGameLayer)
                return "Game";

            return string.IsNullOrEmpty(_name) ? "Tiles" : _name;
        }

        private void SetWidth(int value)
        {
            if (_width == value)
                return;

            value = Math.Max(0, value);

            if (value < _width)
            {
                _tiles = GetRectangle(0, 0, value, _height);
            }
            else if (value > _width)
            {
                var newTiles = new MapTile[value * _height];

                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < value; x++)
                    {
                        if (x < _width)
                            newTiles[x + y * value] = _tiles[x + y * _width];
                        else
                            newTiles[x + y * value] = new MapTile();
                    }
                }

                _tiles = newTiles;
            }

            Set(ref _width, value, propertyName: nameof(Width));
            EnsureThumbnailUpdate();
        }

        private void SetHeight(int value)
        {
            if (_height == value)
                return;

            if (value < 0)
                value = 0;

            if (value < _height)
            {
                _tiles = GetRectangle(0, 0, _width, value);
            }
            else if (value > _height)
            {
                Array.Resize(ref _tiles, _width * value);

                for (int y = _height; y < value; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        _tiles[x + y * _width] = new MapTile();
                    }
                }
            }

            Set(ref _height, value, propertyName: nameof(Height));
            EnsureThumbnailUpdate();
        }

        public void SetColorWithExpectation(Color color)
        {
            if (_isColorExpectationStarted == false)
            {
                _expectationOldColor = _color;
                _isColorExpectationStarted = true;
            }

            Color = color;
            
            Task.Delay(1000).ContinueWith(_ =>
            {
                if (color != Color)
                    return;

                var ignoredAction = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => 
                {
                    var tmp = _color;
                    _color = _expectationOldColor;
                    Set(ref _color, color, nameof(_color), nameof(Color));
                    _color = tmp;

                    _isColorExpectationStarted = false;
                });

                EnsureThumbnailUpdate();
            });
        }

        private void SetImage(MapImage image)
        {
            if (_image != null)
            {
                _image.RemoveCarrier(this);
                _image.Loaded -= Image_Loaded;
            }

            Set(ref _image, image, nameof(_image), nameof(Image));

            if (_image != null)
            {
                _image.AddCarrier(this);
                _image.Loaded += Image_Loaded;

                EnsureThumbnailUpdate();
            }
        }

        public void ResetImage() => Image = null;

        public void SetColorEnvelope(MapEnvelope colorEnvelope)
        {
            _colorEnvelope?.RemoveCarrier(this);
            
            Set(ref _colorEnvelope, colorEnvelope, nameof(_colorEnvelope), nameof(ColorEnvelope));

            _colorEnvelope?.AddCarrier(this);
        }

        public void ResetColorEnvelope() => ColorEnvelope = null;

        #endregion

        public bool TryDrawThumbnail()
        {
            if (_image == null || _image.CanvasBitmap == null)
                return false;

            IsThumbnailBeingDrawn = true;

            var device = CanvasDevice.GetSharedDevice();

            Thumbnail = new CanvasCommandList(device);

            using (CanvasDrawingSession cds = Thumbnail.CreateDrawingSession())
            {
                using (var sb = cds.CreateSpriteBatch())
                {
                    for (int x = 0; x < Width; x++)
                    {
                        for (int y = 0; y < Height; y++)
                        {
                            var tile = _tiles[y * Width + x];

                            if (tile.Index == 0)
                                continue;

                            CanvasSpriteFlip flip = CanvasSpriteFlip.None;
                            float rotateAngle = 0f;

                            if (tile.IsHorizontallyFlipped && tile.IsVerticallyFlipped)
                            {
                                flip = CanvasSpriteFlip.Both;
                            }
                            else if (tile.IsHorizontallyFlipped && !tile.IsVerticallyFlipped)
                            {
                                flip = CanvasSpriteFlip.Vertical;
                            }
                            else if (!tile.IsHorizontallyFlipped && tile.IsVerticallyFlipped)
                            {
                                flip = CanvasSpriteFlip.Horizontal;
                            }

                            if (tile.IsRotated)
                            {
                                rotateAngle = (float)(90 * Math.PI / 180d);
                            }

                            int xTexCoord = tile.Index % 16;
                            int yTexCoord = tile.Index / 16;

                            float factor = 0.2f;
                            float unitSize = RenderingUtilities.GridUnitSize * factor;
                            float unitCenter = RenderingUtilities.GridUnitSize / 2f * factor;
                            float originUnitSize = 64;

                            sb.DrawFromSpriteSheet(
                                _image.CanvasBitmap,
                                new Vector2(x * unitSize + unitCenter, y * unitSize + unitCenter),
                                new Rect(xTexCoord * originUnitSize, yTexCoord * originUnitSize, originUnitSize, originUnitSize),
                                new Vector4(ColorChannels[0], ColorChannels[1], ColorChannels[2], ColorChannels[3]) / 255.0f,
                                new Vector2(unitCenter),
                                rotateAngle,
                                new Vector2(factor / (originUnitSize / RenderingUtilities.GridUnitSize)),
                                flip);
                        }
                    }
                }
            }

            IsThumbnailBeingDrawn = false;

            return true;
        }

        public void ShiftLeft()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (x == Width - 1)
                    {
                        Tiles[x + y * Width] = new MapTile(0, 0, 0, 0);
                        continue;
                    }

                    Tiles[x + y * Width] = Tiles[(x + 1) + y * Width];
                }
            }

            EnsureThumbnailUpdate();
            RaiseModification();
        }

        public void ShiftRight()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = Width - 1; x >= 0; x--)
                {
                    if (x == 0)
                    {
                        Tiles[x + y * Width] = new MapTile(0, 0, 0, 0);
                        continue;
                    }

                    Tiles[x + y * Width] = Tiles[(x - 1) + y * Width];
                }
            }

            EnsureThumbnailUpdate();
            RaiseModification();
        }

        public void ShiftTop()
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (y == Height - 1)
                    {
                        Tiles[x + y * Width] = new MapTile(0, 0, 0, 0);
                        continue;
                    }

                    Tiles[x + y * Width] = Tiles[x + (y + 1) * Width];
                }
            }

            EnsureThumbnailUpdate();
            RaiseModification();
        }

        public void ShiftBottom()
        {
            for (int y = Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (y == 0)
                    {
                        Tiles[x + y * Width] = new MapTile(0, 0, 0, 0);
                        continue;
                    }

                    Tiles[x + y * Width] = Tiles[x + (y - 1) * Width];
                }
            }

            EnsureThumbnailUpdate();
            RaiseModification();
        }

        public MapTile[] GetRectangle(int startX, int startY, int width = 1, int height = 1)
        {
            startX = Math.Max(0, Math.Min(startX, Width - 1));
            startY = Math.Max(0, Math.Min(startY, Height - 1));
            width = Math.Max(1, Math.Min(width, Width - startX));
            height = Math.Max(1, Math.Min(height, Height - startY));

            var newTiles = new MapTile[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    newTiles[x + y * width] = _tiles[x + startX + (y + startY) * Width];
                }
            }

            return newTiles;
        }

        public void ReplaceTiles(int startX, int startY, int width, int height, MapTile[] tiles)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (startX + x < 0 || startX + x >= _width || startY + y < 0 || startY + y >= _height)
                        continue;

                    var sourceTile = tiles[x + y * width];
                    var destTile = new MapTile(sourceTile.Index, sourceTile.Flags, 0, sourceTile.Reserved);

                    _tiles[(startX + x) + (startY + y) * _width] = destTile;
                }
            }

            var ignoredAction = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => RaiseModification());
        }

        public void EnsureThumbnailUpdate()
        {
            var thumbnailDrawed = TryDrawThumbnail();

            if (thumbnailDrawed == false)
                return;

            RaiseVisualChanges();
        }

        private void Image_Loaded(object sender, EventArgs e)
            => EnsureThumbnailUpdate();

        private void MapTilesLayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Color))
                ColorChannels = Color.ToArray();
        }

        protected override ILayerDrawStrategy GetDrawStrategy()
        {
            if (_drawStrategy == null)
                _drawStrategy = new TilesLayerDrawStrategy();

            _drawStrategy.SetLayer(this);

            return _drawStrategy;
        }
    }
}