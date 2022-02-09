using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using System.Numerics;
using System;
using Teeditor.Common.Models.Commands;
using Teeditor.Common.Models.Bindable;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.Commands
{
    internal class TilesPaintCommand : BindableBase, IUndoRedoableCommand
    {
        private MapTilesLayer _layer;

        private int _width;
        private int _height;
        private int _startX;
        private int _startY;

        private MapTile[] _originalTiles;
        private MapTile[] _croppedOriginalTiles;
        private MapTile[] _modifiedTiles;

        private TilesLayerSelection _selection;
        private Vector2 _minPos;
        private Vector2 _maxPos;
        private bool _isEnded;

        private bool _isExecuted = true;
        private bool _isLastExecuted = true;

        public string Name => $"Painting Tiles ({_width}x{_height})";

        public bool IsExecuted
        {
            get => _isExecuted;
            set => Set(ref _isExecuted, value);
        }

        public bool IsLastExecuted
        {
            get => _isLastExecuted;
            set => Set(ref _isLastExecuted, value);
        }

        public TilesPaintCommand(MapTilesLayer layer) => _layer = layer;
        
        public void PaintingStarted(TilesLayerSelection selection)
        {
            _isEnded = false;
            _selection = selection;
            _originalTiles = new MapTile[_layer.Width * _layer.Height];
            _minPos = _maxPos = new Vector2(-1);

            for (int y = 0; y < _layer.Height; y++)
            {
                for (int x = 0; x < _layer.Width; x++)
                {
                    var srcTile = _layer.Tiles[x + y * _layer.Width];
                    _originalTiles[x + y * _layer.Width] = new MapTile(srcTile.Index, srcTile.Flags, 0, srcTile.Reserved);
                }
            }
        }

        public void Paint(int startX, int startY)
        {
            if (_isEnded)
                return;

            if (_minPos.X == -1 || _minPos.Y == -1 || _maxPos.X == -1 || _maxPos.Y == -1)
            {
                _minPos = _maxPos = new Vector2(startX, startY);
            }
            else
            {
                _minPos.X = Math.Min(_minPos.X, startX);
                _minPos.Y = Math.Min(_minPos.Y, startY);
                _maxPos.X = Math.Max(_maxPos.X, startX);
                _maxPos.Y = Math.Max(_maxPos.Y, startY);
            }

            _layer.ReplaceTiles(startX, startY, _selection.Width, _selection.Height, _selection.Tiles);
        }

        public void PaintingEnded()
        {
            _startX = (int)_minPos.X;
            _startY = (int)_minPos.Y;
            _width = (int)(_maxPos.X - _minPos.X + _selection.Width);
            _height = (int)(_maxPos.Y - _minPos.Y + _selection.Height);

            _croppedOriginalTiles = new MapTile[_width * _height];

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (_startX + x < 0 || _startX + x >= _layer.Width || _startY + y < 0 || _startY + y >= _layer.Height)
                        continue;

                    var srcTile = _originalTiles[_startX + x + (_startY + y) * _layer.Width];
                    _croppedOriginalTiles[x + y * _width] = new MapTile(srcTile.Index, srcTile.Flags, 0, srcTile.Reserved);
                }
            }

            _modifiedTiles = new MapTile[_width * _height];

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (_startX + x < 0 || _startX + x >= _layer.Width || _startY + y < 0 || _startY + y >= _layer.Height)
                        continue;

                    var srcTile = _layer.Tiles[_startX + x + (_startY + y) * _layer.Width];
                    _modifiedTiles[x + y * _width] = new MapTile(srcTile.Index, srcTile.Flags, 0, srcTile.Reserved);
                }
            }

            _layer.EnsureThumbnailUpdate();

            _isEnded = true;
            _originalTiles = null;
            _selection = null;
        }

        public void Execute(CommandManager commandManager)
        {
            var ignoredAction = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                commandManager.AddCommand(this);
            });
        }

        public void Redo()
        {
            _layer.ReplaceTiles(_startX, _startY, _width, _height, _modifiedTiles);
            _layer.EnsureThumbnailUpdate();
        }

        public void Undo()
        {
            _layer.ReplaceTiles(_startX, _startY, _width, _height, _croppedOriginalTiles);
            _layer.EnsureThumbnailUpdate();
        }
    }
}
