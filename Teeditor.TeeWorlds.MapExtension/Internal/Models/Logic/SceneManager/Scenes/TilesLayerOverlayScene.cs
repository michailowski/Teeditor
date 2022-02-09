using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Microsoft.Graphics.Canvas;
using Teeditor.Common.Utilities;
using Teeditor_TeeWorlds_Direct3DInterop;
using Teeditor_Direct3DInterop;
using Teeditor_Direct3DInterop.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.Commands;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager.Scenes
{
    internal class TilesLayerOverlayScene : LayerOverlayScene
    {
        private MapTilesLayer _layer;
        private TilesLayerSelection _selection;
        private TextureHandle _textureHandle;

        private TilesPaintCommand _currentPaintCommand;

        private Vector2 _hoveredTilePos;
        private Vector2 _selectionStartTilePos;
        private Vector2 _selectionEndTilePos;
        private bool _isOutOfLayer;

        private Vector2 _selectionTopLeftPos;
        private Vector2 _selectionBottomRightPos;

        private bool _isSpacePressed = false;
        private bool _isMouseCaptured = false;

        private bool _isIdle = true;

        public override bool Activated => !_isIdle;

        public override void SetDependencies(ExplorerSelectionSnapshot selectionSnapshot, SceneCamera camera, Map map)
        {
            var tilesLayer = selectionSnapshot.Layer as MapTilesLayer;

            if (selectionSnapshot.IsEmpty || tilesLayer?.Image?.TextureHandle == null)
            {
                _isIdle = true;
                return;
            }

            _group = selectionSnapshot.Group;
            _layer = tilesLayer;
            _image = _layer.Image;
            _textureHandle = _image.TextureHandle;
            _camera = camera;
            _selection = map.SelectionManager.TilesLayerSelection;

            _isIdle = false;
        }

        #region Mouse and Keyboard Input Methods

        public override void ProcessMouseInput(MouseInput input, out bool handled)
        {
            handled = false;

            if (_isIdle)
                return;

            switch (input.Type)
            {
                case MouseInputType.Move:
                    OnMouseMove(input, out handled);
                    break;

                case MouseInputType.Pressed:
                    OnMousePressed(input, out handled);
                    break;

                case MouseInputType.Released:
                    OnMouseReleased(input, out handled);
                    break;
            }
        }

        private void OnMouseMove(MouseInput input, out bool handled)
        {
            bool leftModifierPressed = (input.KeyModifiers & MouseKeyModifiers.Left) == MouseKeyModifiers.Left;

            RenderingUtilities.PosToWorld(_group.TransformMatrix3X2, input.Position, out var currentPos);

            _hoveredTilePos.X = (int)(currentPos.X / RenderingUtilities.GridUnitSize) * RenderingUtilities.GridUnitSize;
            _hoveredTilePos.Y = (int)(currentPos.Y / RenderingUtilities.GridUnitSize) * RenderingUtilities.GridUnitSize;

            CheckOutOfLayer(currentPos);

            if (_isMouseCaptured && leftModifierPressed)
            {
                if (_selection.IsEmpty && _isOutOfLayer == false)
                {
                    _selectionEndTilePos = _hoveredTilePos;

                    UpdateSelection();
                }
                else
                {
                    if (!PreventLayerChanges)
                        Paint();
                }
            }

            handled = true;
        }

        private void OnMousePressed(MouseInput input, out bool handled)
        {
            if (input.Button == MouseButton.Left)
            {
                if (_selection.IsEmpty && _isOutOfLayer == false)
                {
                    _selectionStartTilePos = _selectionEndTilePos = _hoveredTilePos;

                    UpdateSelection();

                    UserInterface.SetCursorType(CoreCursorType.Cross);
                }
                else
                {
                    if (!PreventLayerChanges)
                    {
                        PaintingStarted();
                        Paint();
                    }
                }

                _isMouseCaptured = true;
            }

            handled = true;
        }

        private void OnMouseReleased(MouseInput input, out bool handled)
        {
            if (input.Button == MouseButton.Left)
            {
                if (_isMouseCaptured && _selection.IsEmpty)
                {
                    Select();
                    UserInterface.SetCursorType(CoreCursorType.Arrow);

                    _isMouseCaptured = false;
                }
                else if (!_selection.IsEmpty)
                {
                    PaintingEnded();
                }
            }
            else if (input.Button == MouseButton.Right)
            {
                if (!_selection.IsEmpty)
                    _selection.Clear();

                _isMouseCaptured = false;
            }

            handled = true;
        }

        public override void ProcessKeyboardInput(KeyboardInput input, out bool handled)
        {
            handled = false;

            if (_isIdle)
                return;

            switch (input.Type)
            {
                case KeyboardInputType.Pressed:
                    OnKeyboardPressed(input, out handled);
                    break;

                case KeyboardInputType.Released:
                    OnKeyboardReleased(input, out handled);
                    break;

            }
        }
        
        private void OnKeyboardPressed(KeyboardInput input, out bool handled)
        {
            if (input.Key == Keys.Space)
            {
                _isSpacePressed = true;
            }

            handled = false;
        }

        private void OnKeyboardReleased(KeyboardInput input, out bool handled)
        {
            if (input.Key == Keys.Space)
            {
                _isSpacePressed = false;
            }

            handled = false;
        }

        #endregion

        #region Draw methods

        public override void Draw(CanvasDrawingSession cds)
        {
            if (_isIdle)
                return;

            if (_group == null || _layer == null)
                return;

            cds.Transform = _group.TransformMatrix3X2;

            if (_isSpacePressed == false)
            {
                DrawLayerBorder(cds);
            }

            if (_isMouseCaptured && _selection.IsEmpty)
            {
                DrawSelectionArea(cds);
                return;
            }

            if (!_isMouseCaptured && _selection.IsEmpty)
            {
                DrawHoveredTileHighlighting(cds);
                return;
            }

            if (!_selection.IsEmpty && !PreventLayerChanges)
            {
                DrawSelectedTiles(cds);
                DrawSelectedTilesBorder(cds);
            }
        }

        private void DrawHoveredTileHighlighting(CanvasDrawingSession cds)
        {
            if (_isOutOfLayer)
                return;

            var highlightRectangle = new Rect
            {
                X = _hoveredTilePos.X,
                Y = _hoveredTilePos.Y,
                Width = RenderingUtilities.GridUnitSize,
                Height = RenderingUtilities.GridUnitSize
            };

            cds.FillRectangle(highlightRectangle, Color.FromArgb(85, 255, 255, 255));
        }

        private void DrawSelectionArea(CanvasDrawingSession cds)
        {
            var selectionRectangle = new Rect(
                    new Point(_selectionTopLeftPos.X, _selectionTopLeftPos.Y),
                    new Point(_selectionBottomRightPos.X, _selectionBottomRightPos.Y));

            cds.FillRectangle(selectionRectangle, Color.FromArgb(125, 255, 255, 255));
        }

        private void DrawSelectedTiles(CanvasDrawingSession cds)
        {
            // TODO : запретить выбор кисти при неустановленном изображении
            // TODO : исправить выход за границы массива tiles при повороте выделенных тайлов

            _graphicsComponent.SetRenderTarget(cds);

            var renderer = _graphicsComponent.GetDefferedRenderer();

            renderer.SetView(_group.TransformMatrix4X4);

            renderer.SetTexture(_textureHandle);

            renderer.DrawBegin(DrawType.Tiles);

            float conv = 1 / 255f;
            float alpha = _layer.ColorChannels[3] * conv;

            renderer.SetColor(_layer.ColorChannels[0] * conv * alpha, _layer.ColorChannels[1] * conv * alpha, _layer.ColorChannels[2] * conv * alpha, alpha);

            for (int y = 0; y < _selection.Height; y++)
            {
                for (int x = 0; x < _selection.Width; x++)
                {
                    var tile = _selection.Tiles[x + y * _selection.Width];

                    var points = RenderingUtilities.GetTilePos(x, y);
                    var texCoords = RenderingUtilities.GetDefaultTextureCoordinates();

                    if (tile.IsVerticallyFlipped && !_layer.IsGameLayer)
                    {
                        RenderingUtilities.FlipTextureCoordinatesVertically(ref texCoords);
                    }

                    if (tile.IsHorizontallyFlipped && !_layer.IsGameLayer)
                    {
                        RenderingUtilities.FlipTextureCoordinatesHorizontally(ref texCoords);
                    }

                    if (tile.IsRotated && !_layer.IsGameLayer)
                    {
                        RenderingUtilities.RotateTextureCoordinates(ref texCoords);
                    }

                    renderer.SetTextureCoordinates(
                        texCoords[0],
                        texCoords[1],
                        texCoords[2],
                        texCoords[3],
                        tile.Index);

                    renderer.Draw(
                            points[0] + _hoveredTilePos,
                            points[1] + _hoveredTilePos,
                            points[2] + _hoveredTilePos,
                            points[3] + _hoveredTilePos);
                }
            }

            renderer.DrawEnd();

            _graphicsComponent.ExecuteCommandLists();
        }

        private void DrawSelectedTilesBorder(CanvasDrawingSession cds)
        {
            var fringeRectangle = new Rect
            {
                X = _hoveredTilePos.X,
                Y = _hoveredTilePos.Y,
                Width = _selection.Width * RenderingUtilities.GridUnitSize,
                Height = _selection.Height * RenderingUtilities.GridUnitSize
            };

            var strokeWidth = 1f / _camera.Scale * 2;

            cds.Antialiasing = CanvasAntialiasing.Aliased;

            cds.DrawRectangle(fringeRectangle, Colors.White, strokeWidth);

            cds.Antialiasing = CanvasAntialiasing.Antialiased;
        }

        private void DrawLayerBorder(CanvasDrawingSession cds)
        {
            var fringeRectangle = new Rect
            {
                X = 0,
                Y = 0,
                Width = _layer.Width * RenderingUtilities.GridUnitSize,
                Height = _layer.Height * RenderingUtilities.GridUnitSize
            };

            var strokeWidth = 2f / _camera.Scale;

            cds.Antialiasing = CanvasAntialiasing.Aliased;

            cds.DrawRectangle(fringeRectangle, Colors.White, strokeWidth);

            cds.Antialiasing = CanvasAntialiasing.Antialiased;
        }

        #endregion

        #region Logic methods

        private void Paint()
        {
            int x = (int)(_hoveredTilePos.X / RenderingUtilities.GridUnitSize);
            int y = (int)(_hoveredTilePos.Y / RenderingUtilities.GridUnitSize);

            _currentPaintCommand?.Paint(x, y);
        }

        private void PaintingStarted()
        {
            _currentPaintCommand = new TilesPaintCommand(_layer);
            _currentPaintCommand.PaintingStarted(_selection);
        }

        private void PaintingEnded()
        {
            _currentPaintCommand.PaintingEnded();

            ExecuteCommand(_currentPaintCommand);

            _currentPaintCommand = null;
        }

        private void Select()
        {
            var srcWidth = _layer.Width;
            var srcHeight = _layer.Height;

            int startX = (int)(_selectionTopLeftPos.X / RenderingUtilities.GridUnitSize);
            int startY = (int)(_selectionTopLeftPos.Y / RenderingUtilities.GridUnitSize);

            startX = Math.Max(0, Math.Min(srcWidth, startX));
            startY = Math.Max(0, Math.Min(srcHeight, startY));

            int endX = (int)(_selectionBottomRightPos.X / RenderingUtilities.GridUnitSize);
            int endY = (int)(_selectionBottomRightPos.Y / RenderingUtilities.GridUnitSize);

            endX = Math.Max(0, Math.Min(srcWidth, endX));
            endY = Math.Max(0, Math.Min(srcHeight, endY));

            var selectionRect = new Rect()
            {
                X = Math.Min(startX, endX),
                Y = Math.Min(startY, endY),
                Width = Math.Abs(endX - startX),
                Height = Math.Abs(endY - startY)
            };

            if (selectionRect.Width == 0 || selectionRect.Height == 0)
                return;

            _selection.Select(_layer, selectionRect);
            _selectionTopLeftPos = _selectionBottomRightPos = _selectionStartTilePos = _selectionEndTilePos = Vector2.Zero;
        }

        private void UpdateSelection()
        {
            _selectionTopLeftPos = new Vector2(
                Math.Min(_selectionEndTilePos.X, _selectionStartTilePos.X),
                Math.Min(_selectionEndTilePos.Y, _selectionStartTilePos.Y));

            _selectionBottomRightPos = new Vector2(
                Math.Max(_selectionEndTilePos.X, _selectionStartTilePos.X) + RenderingUtilities.GridUnitSize,
                Math.Max(_selectionEndTilePos.Y, _selectionStartTilePos.Y) + RenderingUtilities.GridUnitSize);
        }

        private void CheckOutOfLayer(Vector2 position)
        {
            var tileX = (int)(position.X / RenderingUtilities.GridUnitSize);
            var tileY = (int)(position.Y / RenderingUtilities.GridUnitSize);

            _isOutOfLayer = position.X < 0 || position.Y < 0 || tileX >= _layer.Width || tileY >= _layer.Height;
        }

        #endregion
    }
}
