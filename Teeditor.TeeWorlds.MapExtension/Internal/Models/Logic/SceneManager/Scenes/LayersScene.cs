using System.Numerics;
using Microsoft.Graphics.Canvas;
using System.Collections.Generic;
using System.Threading.Tasks;
using Teeditor_TeeWorlds_Direct3DInterop;
using Teeditor_Direct3DInterop;
using Teeditor_Direct3DInterop.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager.Drawers;
using Teeditor.Common.Models.Scene;
using Teeditor.Common.Models.Components;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager.Scenes
{
    internal class LayersScene : IScene
    {
        private GraphicsComponent _graphicsComponent;

        private Map _map;
        private PropertiesManagerSceneAdapter _propertiesManagerAdapter;

        private SceneCamera _camera;
        private SceneTimer _timer;

        private Matrix3x2 _clippingTransformMatrix3x2;
        private List<Task> _renderTasks = new List<Task>();

        private bool _preventDrawing = false;
        private bool _isIdle = true;

        public bool Activated => !_preventDrawing && !_isIdle;

        public void SetComponentsManager(IComponentsManager componentsManager)
        {
            var mapComponentsManager = componentsManager as ComponentsManager;

            _graphicsComponent = mapComponentsManager.GraphicsComponent;
        }

        public void SetPropertiesManagerAdapter(PropertiesManagerSceneAdapter propertiesManagerAdapter)
        {
            _propertiesManagerAdapter = propertiesManagerAdapter;
        }

        public void SetDependencies(Map map, SceneCamera camera, SceneTimer timer)
        {
            if (map == null || camera == null)
            {
                _map = null;
                _camera = null;
                _timer = null;

                _isIdle = true;
                return;
            }

            _map = map;
            _camera = camera;
            _timer = timer;

            _isIdle = false;
        }

        #region Mouse and Keyboard Input Methods

        public void ProcessMouseInput(MouseInput input, out bool handled)
        {
            handled = false;

            if (_isIdle)
                return;

            _camera.ProcessMouseInput(input, out handled);
        }

        public void ProcessKeyboardInput(KeyboardInput input, out bool handled)
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

            if (input.Key == Keys.Space)
            {
                if (_map.CurrentExplorerSelection.Layer is MapTilesLayer)
                {
                    _preventDrawing = true;
                }
                else
                {
                    handled = true;
                }
            }
        }

        private void OnKeyboardReleased(KeyboardInput input, out bool handled)
        {
            handled = false;

            if (input.Key == Keys.Space)
            {
                _preventDrawing = false;
            }
        }

        #endregion

        #region Update methods

        public void Update()
        {
            if (_isIdle || _preventDrawing)
                return;

            UpdateTransformMatricies();
        }

        private void UpdateTransformMatricies()
        {
            var groups = _map.GroupedLayersContainer.Groups;

            Matrix3x2 translationToCenter3x2 = Matrix3x2.CreateTranslation(CanvasProperties.Center);
            Matrix3x2 scale3x2 = Matrix3x2.CreateScale(_camera.Scale, CanvasProperties.Center);

            Vector3 scaleVector = new Vector3(_camera.Scale * 2, _camera.Scale * 2, 0f);
            Vector3 conversionVector = new Vector3(1f / CanvasProperties.Width, -1f / CanvasProperties.Height, 0f);
            Matrix4x4 scale4x4 = Matrix4x4.CreateScale(scaleVector);

            for (int i = 0; i < groups.Count; i++)
            {
                // 3x2 matricies for Win2D
                Vector2 parallax = groups[i].Parallax / 100.0f;
                Vector2 translationVector3x2 = -_camera.Position * parallax - groups[i].Offset;
                Matrix3x2 translation3x2 = Matrix3x2.CreateTranslation(translationVector3x2);
                groups[i].TransformMatrix3X2 = translation3x2 * translationToCenter3x2 * scale3x2;

                // 4x4 matricies for Direct3DInterop
                Vector3 translationVector4x4 = new Vector3(translationVector3x2, 0f);
                Matrix4x4 translation4x4 = Matrix4x4.CreateTranslation(translationVector4x4 * conversionVector);
                groups[i].TransformMatrix4X4 = translation4x4 * scale4x4;
            }

            Matrix3x2 clippingTranslation3x2 = Matrix3x2.CreateTranslation(-_camera.Position);
            _clippingTransformMatrix3x2 = clippingTranslation3x2 * translationToCenter3x2 * scale3x2;
        }

        #endregion

        #region Draw methods

        public void Draw(CanvasDrawingSession cds)
        {
            if (_isIdle || _preventDrawing)
                return;

            DrawLayers(cds);
        }

        private void DrawLayers(CanvasDrawingSession cds)
        {
            _graphicsComponent.SetRenderTarget(cds);

            _renderTasks.Clear();

            MapGroup gameGroup = null;
            MapTilesLayer gameLayer = null;

            var groups = _map.GroupedLayersContainer.Groups;

            for (int i = 0; i < groups.Count; i++)
            {
                if (!groups[i].IsVisible)
                    continue;

                RenderingUtilities.ScreenToWorld(groups[i].TransformMatrix3X2, out var startPos, out var endPos);

                for (int j = 0; j < groups[i].Layers.Count; j++)
                {
                    var layer = groups[i].Layers[j];

                    if (!layer.IsVisible)
                        continue;

                    if (layer.IsHighDetail && !_propertiesManagerAdapter.IsHighDetailEnabled)
                        continue;

                    if (layer is MapTilesLayer tilesLayer && tilesLayer.IsGameLayer)
                    {
                        gameGroup = groups[i];
                        gameLayer = tilesLayer;

                        continue;
                    }

                    var clippingTopLeft = new Vector2((float)groups[i].Clip.Left, (float)groups[i].Clip.Top);
                    var clippingBottomRight = new Vector2((float)groups[i].Clip.Right, (float)groups[i].Clip.Bottom);

                    if (groups[i].UseClipping)
                    {
                        RenderingUtilities.WorldToPos(_clippingTransformMatrix3x2, clippingTopLeft, out clippingTopLeft);
                        RenderingUtilities.WorldToPos(_clippingTransformMatrix3x2, clippingBottomRight, out clippingBottomRight);
                    }

                    var args = new DrawingTaskArgs()
                    {
                        GraphicsComponent = _graphicsComponent,
                        TransformMatrix4x4 = groups[i].TransformMatrix4X4,
                        StartPos = startPos,
                        EndPos = endPos,
                        Time = _timer.Time,
                        Map = _map,
                        UseClipping = groups[i].UseClipping,
                        ClippingTopLeft = clippingTopLeft,
                        ClippingBottomRight = clippingBottomRight
                    };

                    var isDrawed = layer.DrawStrategy.TryGetDrawingTask(out var drawingTask, args);

                    if (isDrawed)
                        _renderTasks.Add(drawingTask);
                }
            }

            // Game layer drawing in the foreground
            if (gameLayer != null && gameGroup != null)
            {
                RenderingUtilities.ScreenToWorld(gameGroup.TransformMatrix3X2, out var startPos, out var endPos);

                var clippingTopLeft = new Vector2((float)gameGroup.Clip.Left, (float)gameGroup.Clip.Top);
                var clippingBottomRight = new Vector2((float)gameGroup.Clip.Right, (float)gameGroup.Clip.Bottom);

                if (gameGroup.UseClipping)
                {
                    RenderingUtilities.WorldToPos(_clippingTransformMatrix3x2, clippingTopLeft, out clippingTopLeft);
                    RenderingUtilities.WorldToPos(_clippingTransformMatrix3x2, clippingBottomRight, out clippingBottomRight);
                }

                var args = new DrawingTaskArgs()
                {
                    GraphicsComponent = _graphicsComponent,
                    TransformMatrix4x4 = gameGroup.TransformMatrix4X4,
                    StartPos = startPos,
                    EndPos = endPos,
                    Time = _timer.Time,
                    Map = _map,
                    UseClipping = gameGroup.UseClipping,
                    ClippingTopLeft = clippingTopLeft,
                    ClippingBottomRight = clippingBottomRight
                };

                var isDrawed = gameLayer.DrawStrategy.TryGetDrawingTask(out var drawingTask, args);

                if (isDrawed)
                    _renderTasks.Add(drawingTask);
            }

            var layersRenderResultTask = Task.WhenAll(_renderTasks);

            layersRenderResultTask.Wait();

            if (layersRenderResultTask.Status == TaskStatus.RanToCompletion)
                _graphicsComponent.ExecuteCommandLists();
        }

        #endregion
    }
}
