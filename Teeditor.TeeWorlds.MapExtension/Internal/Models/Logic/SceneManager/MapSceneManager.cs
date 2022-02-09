using Teeditor_Direct3DInterop;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.UI;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager.Scenes;
using Teeditor.Common.Models.Commands;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Models.Scene;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager
{
    internal class MapSceneManager : ISceneManager
    {
        private Map _map;
        private ExplorerSelectionSnapshot _selectionSnapshot;

        private static CanvasImageBrush _checkeredBackgroundBrush;
        private static PropertiesManagerSceneAdapter _propertiesManagerAdapter;
        private static ComponentsManager _componentsManager;

        private static LayersScene _layersScene;
        private static TilesPaletteScene _tilesPaletteScene;
        private static List<LayerOverlayScene> _layerOverlayScenes;

        public SceneCamera Camera { get; } = new SceneCamera();
        public SceneTimer Timer { get; } = new SceneTimer();
        public bool IsIdle { get; private set; } = true;

        public event CommandExecuteHandler CommandExecuted;

        public MapSceneManager()
        {
            InitCheckeredBackgroundBrushLazy();
            InitPropertiesManagerAdapterLazy();
            InitLayersSceneLazy();
            InitTilesPaletteSceneLazy();
            InitLayerOverlayScenesLazy();

            _layersScene.SetPropertiesManagerAdapter(_propertiesManagerAdapter);

            foreach (var overlayScene in _layerOverlayScenes)
            {
                overlayScene.CommandExecuted += LayerOverlay_CommandExecuted;
            }
        }

        #region Init Methods

        private static void InitCheckeredBackgroundBrushLazy()
        {
            if (_checkeredBackgroundBrush != null)
                return;

            var device = CanvasDevice.GetSharedDevice();

            var firstChecker = Color.FromArgb(255, 153, 153, 153);
            var secondChecker = Color.FromArgb(255, 186, 186, 186);
            var checkers = new Color[] { firstChecker, secondChecker, secondChecker, firstChecker };
            var checkersBitmap = CanvasBitmap.CreateFromColors(device, checkers, 2, 2);

            _checkeredBackgroundBrush = new CanvasImageBrush(device, checkersBitmap)
            {
                ExtendX = CanvasEdgeBehavior.Wrap,
                ExtendY = CanvasEdgeBehavior.Wrap,
                Transform = Matrix3x2.CreateScale(16),
                Interpolation = CanvasImageInterpolation.NearestNeighbor
            };
        }

        private static void InitPropertiesManagerAdapterLazy()
        {
            if (_propertiesManagerAdapter != null)
                return;

            _propertiesManagerAdapter = new PropertiesManagerSceneAdapter();
        }

        private static void InitLayersSceneLazy()
        {
            if (_layersScene != null)
                return;

            _layersScene = new LayersScene();
        }

        private static void InitTilesPaletteSceneLazy()
        {
            if (_tilesPaletteScene != null)
                return;

            _tilesPaletteScene = new TilesPaletteScene();
        }

        private static void InitLayerOverlayScenesLazy()
        {
            if (_layerOverlayScenes != null)
                return;

            _layerOverlayScenes = new List<LayerOverlayScene>()
            {
                new TilesLayerOverlayScene(),
                new QuadsLayerOverlayScene()
            };
        }

        #endregion

        public void SetTab(ITab tab)
        {
            _map = tab.Data as Map;
            _selectionSnapshot = new ExplorerSelectionSnapshot(_map);

            var mapTab = tab as Tab;

            _componentsManager = mapTab?.ComponentsManager as ComponentsManager;
            _componentsManager.GraphicsComponent.SetTextureManager(_map.ImagesContainer.TexturesManager);

            SetComponents();

            _propertiesManagerAdapter.SetManager(tab.PropertiesManager);

            _layersScene.SetDependencies(_map, Camera, Timer);

            IsIdle = false;
        }

        public void SetSize(Vector2 size)
            => _componentsManager?.GraphicsComponent.SetViewportSize(size.X, size.Y);
        
        private void LayerOverlay_CommandExecuted(object sender, IUndoRedoableCommand command)
            => CommandExecuted?.Invoke(sender, command);

        private void SetComponents()
        {
            _layersScene.SetComponentsManager(_componentsManager);
            _tilesPaletteScene.SetComponentsManager(_componentsManager);

            foreach (var overlayScene in _layerOverlayScenes)
            {
                overlayScene.SetComponentsManager(_componentsManager);
            }
        }

        #region Mouse and Keyboard Input Methods

        public void ProcessMouseInput(MouseInput input)
        {
            _layersScene.ProcessMouseInput(input, out bool handled);

            if (handled)
                return;

            _tilesPaletteScene.ProcessMouseInput(input, out handled);

            foreach (var overlayScene in _layerOverlayScenes)
            {
                overlayScene.ProcessMouseInput(input, out handled);

                if (handled)
                    return;
            }
        }

        public void ProcessKeyboardInput(KeyboardInput input)
        {
            _layersScene.ProcessKeyboardInput(input, out bool handled);

            if (handled)
                return;

            _tilesPaletteScene.ProcessKeyboardInput(input, out handled);

            if (handled)
                return;

            foreach (var overlayScene in _layerOverlayScenes)
            {
                overlayScene.ProcessKeyboardInput(input, out handled);

                if (handled)
                    return;
            }
        }

        #endregion

        #region Update Methods

        public void Update()
        {
            _layersScene.Update();

            _selectionSnapshot = new ExplorerSelectionSnapshot(_map);

            UpdateTilePalette();

            EnsureTilePaletteOverlayTransition();

            UpdateLayerOverlayScenes();

            Timer.Tick();
        }

        private void UpdateTilePalette()
        {
            _tilesPaletteScene.SetDependencies(_selectionSnapshot);
            _tilesPaletteScene.Update();
        }

        private void EnsureTilePaletteOverlayTransition()
        {
            if (_tilesPaletteScene.Activated)
            {
                foreach (var overlayScene in _layerOverlayScenes)
                {
                    overlayScene.PreventLayerChanges = true;
                }

                _selectionSnapshot = new ExplorerSelectionSnapshot(TilesPaletteScene.ImitatedGroup, TilesPaletteScene.ImitatedTileLayer);
            }
            else
            {
                foreach (var overlayScene in _layerOverlayScenes)
                {
                    overlayScene.PreventLayerChanges = false;
                }
            }
        }

        private void UpdateLayerOverlayScenes()
        {
            foreach (var overlayScene in _layerOverlayScenes)
            {
                overlayScene.SetDependencies(_selectionSnapshot, Camera, _map);
                overlayScene.Update();
            }
        }

        #endregion

        #region Draw Methods

        public void Draw(CanvasDrawingSession drawingSession)
        {
            DrawBackground(drawingSession);

            _layersScene.Draw(drawingSession);

            _tilesPaletteScene.Draw(drawingSession);

            foreach (var overlayScene in _layerOverlayScenes)
            {
                overlayScene.Draw(drawingSession);
            }

            if (_tilesPaletteScene.IsActive == true)
                return;
            
            if (_propertiesManagerAdapter.IsGridEnabled && _selectionSnapshot.IsEmpty == false)
            {
                DrawGrid(drawingSession);
            }

            if (_propertiesManagerAdapter.IsProofBordersEnabled)
            {
                DrawProofBorders(drawingSession);
            }
        }

        private void DrawBackground(CanvasDrawingSession cds) 
            => cds.FillRectangle(0, 0, CanvasProperties.Width, CanvasProperties.Height, _checkeredBackgroundBrush);

        private void DrawGrid(CanvasDrawingSession cds)
        {
            var colorMain = Colors.Red;
            var colorSub = Colors.DarkRed;

            var strokeWidth = 1f / Camera.Scale;

            var k = Math.Max(1, (int)(1f / Camera.Scale));
            var gridSize = RenderingUtilities.GridUnitSize * k;
            var gridFactor = 4 * gridSize / k;

            RenderingUtilities.ScreenToWorld(_selectionSnapshot.Group.TransformMatrix3X2, out var startPos, out var endPos);

            float startY = (int)(startPos.Y / gridSize - 1) * gridSize;
            float startX = (int)(startPos.X / gridSize - 1) * gridSize;
            float endY = (int)(endPos.Y / gridSize + 1) * gridSize;
            float endX = (int)(endPos.X / gridSize + 1) * gridSize;

            cds.Transform = _selectionSnapshot.Group.TransformMatrix3X2;

            cds.Antialiasing = CanvasAntialiasing.Aliased;

            for (float y = startY; y < endY; y += gridSize)
            {
                var color = y % gridFactor == 0 ? colorMain : colorSub;

                cds.DrawLine(
                    new Vector2(startX, y),
                    new Vector2(endX, y),
                    color,
                    strokeWidth);
            }

            for (float x = startX; x < endX; x += gridSize)
            {
                var color = x % gridFactor == 0 ? colorMain : colorSub;

                cds.DrawLine(
                    new Vector2(x, startY),
                    new Vector2(x, endY),
                    color,
                    strokeWidth);
            }

            cds.Antialiasing = CanvasAntialiasing.Antialiased;
        }

        private void DrawProofBorders(CanvasDrawingSession cds)
        {
            cds.Transform = Matrix3x2.CreateScale(Camera.Scale, CanvasProperties.Center);

            float strokeWidth = 2f / Camera.Scale;

            Vector2 firstLastPoint = Vector2.Zero;
            Vector2 secondLastPoint = Vector2.Zero;

            float start = 1.0f;
            float end = 16.0f / 9.0f;
            const int stepsNumber = 20;

            for (int i = 0; i <= stepsNumber; i++)
            {
                float aspect = start + (end - start) * (i / (float)stepsNumber);

                RenderingUtilities.CalcProofPoints(aspect, out var firstPoint, out var secondPoint);

                cds.Antialiasing = CanvasAntialiasing.Aliased;

                if (i == 0)
                {
                    cds.DrawLine(firstPoint.X, firstPoint.Y, secondPoint.X, firstPoint.Y, Colors.White, strokeWidth);
                    cds.DrawLine(firstPoint.X, secondPoint.Y, secondPoint.X, secondPoint.Y, Colors.White, strokeWidth);
                }

                cds.Antialiasing = CanvasAntialiasing.Antialiased;

                if (i != 0)
                {
                    cds.DrawLine(firstPoint.X, firstPoint.Y, firstLastPoint.X, firstLastPoint.Y, Colors.White, strokeWidth);
                    cds.DrawLine(secondPoint.X, firstPoint.Y, secondLastPoint.X, firstLastPoint.Y, Colors.White, strokeWidth);

                    cds.DrawLine(firstPoint.X, secondPoint.Y, firstLastPoint.X, secondLastPoint.Y, Colors.White, strokeWidth);
                    cds.DrawLine(secondPoint.X, secondPoint.Y, secondLastPoint.X, secondLastPoint.Y, Colors.White, strokeWidth);
                }

                cds.Antialiasing = CanvasAntialiasing.Aliased;

                if (i == stepsNumber)
                {
                    cds.DrawLine(firstPoint.X, firstPoint.Y, firstPoint.X, secondPoint.Y, Colors.White, strokeWidth);
                    cds.DrawLine(secondPoint.X, firstPoint.Y, secondPoint.X, secondPoint.Y, Colors.White, strokeWidth);
                }

                cds.Antialiasing = CanvasAntialiasing.Antialiased;

                firstLastPoint = firstPoint;
                secondLastPoint = secondPoint;
            }

            float[] aspects = { 4.0f / 3.0f, 16.0f / 10.0f, 5.0f / 4.0f, 16.0f / 9.0f };
            Color[] colors = { Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow };

            for (int i = 0; i < 2; i++)
            {
                RenderingUtilities.CalcProofPoints(aspects[i], out var firstPoint, out var secondPoint);

                var x = firstPoint.X;
                var y = firstPoint.Y;
                var width = secondPoint.X - firstPoint.X;
                var height = secondPoint.Y - firstPoint.Y;

                cds.DrawLine(x, y, x + width, y, colors[i], strokeWidth);
                cds.DrawLine(x + width, y, x + width, y + height, colors[i], strokeWidth);
                cds.DrawLine(x + width, y + height, x, y + height, colors[i], strokeWidth);
                cds.DrawLine(x, y + height, x, y, colors[i], strokeWidth);
            }
        }

        #endregion
    }
}
