using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.ApplicationModel.Core;
using Microsoft.Graphics.Canvas;
using Teeditor.Common.Utilities;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Teeditor_Direct3DInterop;
using Teeditor_Direct3DInterop.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using System.Linq;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using System.Collections.Generic;
using Teeditor.Common.Models.Bindable;
using Teeditor.TeeWorlds.MapExtension.Internal.Views.Editor;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Editor;
using Microsoft.Graphics.Canvas.Geometry;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager.Scenes
{
    internal class QuadsLayerOverlayScene : LayerOverlayScene
    {
        private MapQuadsLayer _layer;
        private EnvelopesContainer _envelopesContainer;

        private Vector2 _currentPos;
        private Vector2 _startPos;
        private Vector2 _endPos;
        private Vector2 _currentTile;

        private BrushState _state = BrushState.None;
        private QuadsLayerSelection _selection;
        private List<MapQuad> _copiedQuads;
        private bool _isRotating = false;
        private bool _isShiftPressed = false;
        private bool _gridSticking = false;
        private bool _isControlPressed = false;

        private MapQuad _manipulationStartingQuad;
        private int _manipulationStartingQuadPointId;

        readonly Color _perimeterPointsColor = Colors.Red;
        readonly Color _perimeterPointsHighlightColor = Colors.DarkRed;
        readonly Color _perimeterPointsSelectColor = Colors.IndianRed;
        readonly Color _centerPointColor = Colors.LimeGreen;
        readonly Color _centerPointHighlightColor = Colors.Green;
        readonly Color _centerPointSelectColor = Colors.LightGreen;

        private CanvasDevice _device;

        private bool _isIdle = true;

        public override bool Activated => !_isIdle;

        public QuadsLayerOverlayScene()
        {
            _device = CanvasDevice.GetSharedDevice();
            _copiedQuads = new List<MapQuad>();
        }

        public override void SetDependencies(ExplorerSelectionSnapshot selectionSnapshot, SceneCamera camera, Map map)
        {
            if (selectionSnapshot.Layer is MapQuadsLayer == false)
            {
                _isIdle = true;
                return;
            }

            if (selectionSnapshot.IsEmpty)
            {
                _isIdle = true;
                return;
            }

            _group = selectionSnapshot.Group;
            _layer = (MapQuadsLayer)selectionSnapshot.Layer;
            _image = _layer.Image;
            _camera = camera;
            _selection = map.SelectionManager.QuadsLayerSelection;
            _envelopesContainer = map.EnvelopesContainer;

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
            RenderingUtilities.PosToWorld(_group.TransformMatrix3X2, input.Position, out _currentPos);

            _currentTile.X = (int)(_currentPos.X / RenderingUtilities.GridUnitSize) * RenderingUtilities.GridUnitSize;
            _currentTile.Y = (int)(_currentPos.Y / RenderingUtilities.GridUnitSize) * RenderingUtilities.GridUnitSize;

            if (input.KeyModifiers.HasFlag(MouseKeyModifiers.Left))
            {
                _endPos = _currentPos;

                if (_state == BrushState.Moving)
                    MovePoints();
            }

            handled = true;
        }

        private void OnMousePressed(MouseInput input, out bool handled)
        {
            if (input.Button == MouseButton.Left)
            {
                _endPos = _startPos = _currentPos;

                if (IntersectsSelectedPoints(_startPos.ToPoint()))
                {
                    _state = BrushState.Moving;

                    GetHoveredQuadPoint(out _manipulationStartingQuad, out _manipulationStartingQuadPointId);

                    _selection.FlushUpdates();
                }
                else
                {
                    if ((input.KeyModifiers & MouseKeyModifiers.Shift) != MouseKeyModifiers.Shift)
                    {
                        _selection.Clear();
                    }

                    Select();

                    if (_selection.IsEmpty == false)
                    {
                        _state = BrushState.Moving;
                    }
                    else
                    {
                        _state = BrushState.Selection;
                        UserInterface.SetCursorType(CoreCursorType.Cross);
                    }
                }
            }

            handled = true;
        }

        private void OnMouseReleased(MouseInput input, out bool handled)
        {
            if (input.Button == MouseButton.Left)
            {
                if (_state == BrushState.Selection)
                {
                    Select();
                    UserInterface.SetCursorType(CoreCursorType.Arrow);

                    _state = BrushState.None;
                }
                else if (_state == BrushState.Moving)
                {
                    _selection.FlushUpdates();
                    _state = BrushState.None;
                }
            }
            else if (input.Button == MouseButton.Right)
            {
                _startPos = _currentPos;
                TryShowContextMenu();
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
            handled = false;

            if (input.Key == Keys.LeftAlt)
            {
                _gridSticking = true;
                handled = true;
            }
            else if (input.Key == Keys.R && _isShiftPressed == true)
            {
                _isRotating = true;
                handled = true;
            }
            else if (input.Key == Keys.LeftShift || input.Key == Keys.RightShift)
            {
                _isShiftPressed = true;
                handled = true;
            }
            else if (input.Key == Keys.LeftControl || input.Key == Keys.RightControl)
            {
                _isControlPressed = true;
                handled = true;
            }
            else if (input.Key == Keys.C)
            {
                if (_isControlPressed)
                {
                    CopySelectedQuads();
                }
            }
            else if (input.Key == Keys.V)
            {
                if (_isControlPressed)
                {
                    PasteSelectedQuads();
                }
            }
            else if (input.Key == Keys.Delete)
            {
                RemoveSelectedQuads();
            }
        }

        private void OnKeyboardReleased(KeyboardInput input, out bool handled)
        {
            handled = false;

            if (input.Key == Keys.LeftAlt)
            {
                _gridSticking = false;
            }
            else if (input.Key == Keys.R)
            {
                _isRotating = false;
                handled = true;
            }
            else if (input.Key == Keys.LeftShift || input.Key == Keys.RightShift)
            {
                _isShiftPressed = false;
                handled = true;
            }
            else if (input.Key == Keys.LeftControl || input.Key == Keys.RightControl)
            {
                _isControlPressed = false;
                handled = true;
            }
        }

        #endregion

        #region Draw methods

        public override void Draw(CanvasDrawingSession cds)
        {
            if (_isIdle)
                return;

            cds.Transform = _group.TransformMatrix3X2;

            cds.Antialiasing = CanvasAntialiasing.Aliased;

            DrawPoints(cds);

            if (_state == BrushState.Selection)
            {
                DrawSelectionArea(cds);
            }

            cds.Antialiasing = CanvasAntialiasing.Antialiased;
        }

        private void DrawPoints(CanvasDrawingSession cds)
        {
            var pointSize = 8f / _camera.Scale;

            for (int i = 0; i < _layer.Quads.Count; i++)
            {
                var color = Colors.Transparent;
                var strokeWidth = 1f / _camera.Scale;
                var pointRect = new Rect();

                var isQuadSelected = _selection.HasPoint(_layer.Quads[i], 4);

                if (isQuadSelected == true)
                {
                    var selectedQuadFillColor = Colors.White;
                    selectedQuadFillColor.A = 30;

                    var geometryPoints = new Vector2[4] 
                    {
                        _layer.Quads[i].Points[0].Position,
                        _layer.Quads[i].Points[1].Position,
                        _layer.Quads[i].Points[3].Position,
                        _layer.Quads[i].Points[2].Position
                    };

                    var quadGeometry = CanvasGeometry.CreatePolygon(_device, geometryPoints);

                    cds.FillGeometry(quadGeometry, selectedQuadFillColor);
                    cds.DrawGeometry(quadGeometry, Colors.White, strokeWidth);
                }

                // Draw points
                for (int p = 0; p < 5; p++)
                {
                    color = p < 4 ? _perimeterPointsColor : _centerPointColor;

                    pointRect.X = _layer.Quads[i].Points[p].Position.X - pointSize / 2;
                    pointRect.Y = _layer.Quads[i].Points[p].Position.Y - pointSize / 2;
                    pointRect.Width = pointSize;
                    pointRect.Height = pointSize;

                    if (_selection.HasPoint(_layer.Quads[i], p))
                        color = p < 4 ? _perimeterPointsSelectColor : _centerPointSelectColor;
                    else if (pointRect.Contains(_currentPos.ToPoint()))
                        color = p < 4 ? _perimeterPointsHighlightColor : _centerPointHighlightColor;

                    cds.FillRectangle(pointRect, color);
                    cds.DrawRectangle(pointRect, Colors.Black, strokeWidth);
                }
            }
        }

        private void DrawSelectionArea(CanvasDrawingSession cds)
            => cds.DrawRectangle(new Rect(_startPos.ToPoint(), _endPos.ToPoint()), Colors.White, 1f / _camera.Scale);
        
        #endregion

        #region Logic methods

        private void Select()
        {
            var padding = 4f / _camera.Scale;
            var selectionRect = new Rect(_startPos.ToPoint(), _endPos.ToPoint());

            // Let's add an outer indentation to be able to define an intersection with a point selection
            selectionRect.X -= padding;
            selectionRect.Y -= padding;
            selectionRect.Width += padding * 2;
            selectionRect.Height += padding * 2;

            for (int i = 0; i < _layer.Quads.Count; i++)
            {
                for (int p = 0; p < _layer.Quads[i].Points.Length; p++)
                {
                    var point = _layer.Quads[i].Points[p].Position;

                    if (selectionRect.Contains(point.ToPoint()))
                    {
                        _selection.TryAddPoint(_layer.Quads[i], p);

                        // Select only the first point to avoid multiple choices in a situation with overlapping points
                        if (_startPos == _endPos)
                            return;
                    }
                }
            }
        }

        private void CopySelectedQuads()
        {
            _copiedQuads.Clear();
            _copiedQuads.AddRange(_selection.GetQuadsByCenter());
        }

        private async void PasteSelectedQuads()
        {
            if (_copiedQuads.Count == 0)
                return;

            var delta = _copiedQuads[0].Points[4].Position - _currentPos;

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _selection.Clear();

                foreach (var copiedQuad in _copiedQuads)
                {
                    var newQuad = new MapQuad()
                    {
                        ColorEnvIndex = copiedQuad.ColorEnvIndex,
                        ColorEnvOffset = copiedQuad.ColorEnvOffset,
                        PosEnvIndex = copiedQuad.PosEnvIndex,
                        PosEnvOffset = copiedQuad.PosEnvOffset
                    };

                    for (int i = 0; i < 5; i++)
                    {
                        newQuad.Points[i] = new MapQuadPoint();
                        newQuad.Points[i].Color = copiedQuad.Points[i].Color;
                        newQuad.Points[i].Texture = copiedQuad.Points[i].Texture;
                        newQuad.Points[i].Position = copiedQuad.Points[i].Position - delta;
                        newQuad.Points[i].LastPosition = newQuad.Points[i].Position;

                        _selection.TryAddPoint(newQuad, i);
                    }

                    _layer.Quads.Add(newQuad);
                }
            });
        }

        private async void RemoveSelectedQuads()
        {
            var quads = _selection.GetQuadsByCenter();

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                foreach (var quad in quads)
                {
                    _layer.Quads.Remove(quad);
                }
            });
        }

        public static void QuadRotate(MapQuad quad, float rotation)
        {
            var centerPos = quad.Points[4].Position;

            for (int i = 0; i < 4; i++)
            {
                var point = quad.Points[i];

                float x = point.LastPosition.X - centerPos.X;
                float y = point.LastPosition.Y - centerPos.Y;

                var newX = x * MathF.Cos(rotation) - y * MathF.Sin(rotation) + centerPos.X;
                var newY = x * MathF.Sin(rotation) + y * MathF.Cos(rotation) + centerPos.Y;

                point.Position = new Vector2(newX, newY);
            }
        }

        private void MovePoints()
        {
            if (_isRotating)
            {
                var quads = _selection.GetQuadsByCenter();

                float rotateAngle = (_endPos.X - _startPos.X) * 0.002f;

                foreach (var quad in quads)
                {
                    QuadRotate(quad, rotateAngle);
                }
            }
            else
            {
                var quads = _selection.GetQuadsByCenter();

                var deltaPos = Vector2.Zero;

                if (_gridSticking && _manipulationStartingQuad != null && _manipulationStartingQuadPointId >= 0)
                {
                    deltaPos = _currentTile - _manipulationStartingQuad.Points[_manipulationStartingQuadPointId].LastPosition;
                }
                else
                {
                    deltaPos = _endPos - _startPos;
                }

                if (quads.Count > 0)
                {
                    foreach (var q in quads)
                    {
                        if (_isShiftPressed)
                        {
                            q.Points[4].Position = _gridSticking ? _currentTile : q.Points[4].LastPosition + _endPos - _startPos;
                        }
                        else
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                q.Points[i].Position = q.Points[i].LastPosition + deltaPos;
                            }
                        }
                    }
                }
                else
                {
                    var selectedPoints = _selection.GetPoints();

                    foreach (var p in selectedPoints)
                    {
                        p.Position = p.LastPosition + deltaPos;
                    }
                }
            }
        }

        private bool IntersectsSelectedPoints(Point sourcePoint)
        {
            var size = 8f / _camera.Scale;

            Rect rect = new Rect(
                sourcePoint.X - size / 2,
                sourcePoint.Y - size / 2,
                size,
                size);

            return _selection.IntersectsRect(rect);
        }

        private bool GetHoveredQuadPoint(out MapQuad mapQuad, out int quadPointId)
        {
            mapQuad = null;
            quadPointId = -1;

            var size = 8f / _camera.Scale;

            for (int i = 0; i < _layer.Quads.Count; i++)
            {
                Rect pointRect = new Rect();

                for (int p = 0; p < 5; p++)
                {
                    pointRect.X = _layer.Quads[i].Points[p].Position.X - size / 2;
                    pointRect.Y = _layer.Quads[i].Points[p].Position.Y - size / 2;
                    pointRect.Width = size;
                    pointRect.Height = size;

                    if (!pointRect.Contains(_currentPos.ToPoint()))
                        continue;

                    mapQuad = _layer.Quads[i];
                    quadPointId = p;
                    return true;
                }
            }

            return false;
        }

        private async void TryShowContextMenu()
        {
            if (!GetHoveredQuadPoint(out var quad, out var pointId))
                return;

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var flyout = new Flyout();

                var viewModel = new QuadPointPropertiesViewModel(pointId, quad, _layer, _envelopesContainer);

                viewModel.QuadRemoved += delegate (object sender, EventArgs e) 
                { 
                    flyout.Hide(); 
                };

                flyout.Content = new QuadPointPropertiesControl(viewModel);

                var globalPointerPosition = Window.Current.CoreWindow.PointerPosition;
                var position = new Point() 
                { 
                    X = globalPointerPosition.X - Window.Current.Bounds.X, 
                    Y = globalPointerPosition.Y - Window.Current.Bounds.Y 
                };

                FlyoutShowOptions options = new FlyoutShowOptions
                {
                    Position = position,
                    ShowMode = FlyoutShowMode.Transient
                };

                flyout.ShowAt(Window.Current.Content, options);
            });
        }

        #endregion
    }
}
