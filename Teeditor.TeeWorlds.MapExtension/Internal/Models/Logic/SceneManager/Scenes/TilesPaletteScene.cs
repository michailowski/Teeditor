using System;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Teeditor_Direct3DInterop;
using Teeditor_Direct3DInterop.Enumerations;
using Teeditor_TeeWorlds_Direct3DInterop;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.Common.Models.Scene;
using Teeditor.Common.Models.Components;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager.Scenes
{
    internal class TilesPaletteScene : IScene
    {
        private GraphicsComponent _graphicsComponent;

        private TextureHandle _textureHandle;
        private int[] _colorChannels;

        private bool _preventDrawing = true;
        private bool _isIdle = true;

        private bool _hasUpdateAfterLastDraw = false;

        private const int PaletteWidth = 16;
        private const int PaletteHeight = 16;

        private static MapTilesLayer _imitatedTileLayer;
        private static MapGroup _imitatedGroup;

        public static MapTilesLayer ImitatedTileLayer =>
             _imitatedTileLayer ?? (_imitatedTileLayer = ImitatedTilesLayerInitLazy());

        public static MapGroup ImitatedGroup =>
             _imitatedGroup ?? (_imitatedGroup = new MapGroup());

        public bool Activated => !_preventDrawing && _hasUpdateAfterLastDraw;

        public bool IsActive => !_preventDrawing;

        #region Init Methods

        public void SetComponentsManager(IComponentsManager componentsManager)
        {
            var mapComponentsManager = componentsManager as ComponentsManager;

            _graphicsComponent = mapComponentsManager.GraphicsComponent;
        }

        public void SetDependencies(ExplorerSelectionSnapshot selectionSnapshot)
        {
            var tilesLayer = selectionSnapshot.Layer as MapTilesLayer;

            if (selectionSnapshot.IsEmpty || tilesLayer?.Image?.TextureHandle == null)
            {
                _isIdle = true;
                return;
            }

            _colorChannels = tilesLayer.ColorChannels;
            _textureHandle = tilesLayer.Image.TextureHandle;
            ImitatedTileLayer.Image = tilesLayer.Image;
            ImitatedGroup.Offset = CanvasProperties.Center;

            _isIdle = false;
        }

        private static MapTilesLayer ImitatedTilesLayerInitLazy()
        {
            var paletteLayer = new MapTilesLayer(PaletteWidth, PaletteHeight);

            FillPaletteTiles(paletteLayer);

            return paletteLayer;
        }

        private static void FillPaletteTiles(MapTilesLayer layer)
        {
            for (int y = 0; y < layer.Height; y++)
            {
                for (int x = 0; x < layer.Width; x++)
                {
                    int index = x + y * layer.Width;
                    layer.Tiles[index] = new MapTile((byte)(x + y * layer.Width));
                }
            }
        }

        #endregion

        #region Update Methods

        public void Update()
        {
            if (_isIdle || _preventDrawing)
                return;

            UpdatePaletteMatrices();

            _hasUpdateAfterLastDraw = true;
        }

        private void UpdatePaletteMatrices()
        {
            byte textureUnitSize = 64;

            float scale = 0f;
            float maxScale = textureUnitSize / RenderingUtilities.GridUnitSize * 2;

            if (CanvasProperties.Height <= CanvasProperties.Width)
                scale = CanvasProperties.Height / (PaletteHeight * RenderingUtilities.GridUnitSize) * 2;
            else
                scale = CanvasProperties.Width / (PaletteWidth * RenderingUtilities.GridUnitSize) * 2;

            scale = Math.Min(scale, maxScale);

            Vector3 translation4x4 = new Vector3(-1f / scale, 1f / scale, 0f);
            ImitatedGroup.TransformMatrix4X4 = Matrix4x4.CreateTranslation(translation4x4) * Matrix4x4.CreateScale(new Vector3(scale, scale, 0f));

            Vector2 translation3x2 = Vector2.Zero;
            ImitatedGroup.TransformMatrix3X2 = Matrix3x2.CreateTranslation(translation3x2) * Matrix3x2.CreateScale(scale / 2);
        }

        public void ProcessMouseInput(MouseInput input, out bool handled) => handled = false;

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
                _preventDrawing = false;
            }
        }

        private void OnKeyboardReleased(KeyboardInput input, out bool handled)
        {
            handled = false;

            if (input.Key == Keys.Space)
            {
                _preventDrawing = true;
            }
        }

        #endregion

        #region Draw Methods

        public void Draw(CanvasDrawingSession cds)
        {
            if (_isIdle || _preventDrawing)
                return;

            _graphicsComponent.SetRenderTarget(cds);

            var renderer = _graphicsComponent.GetDefferedRenderer();

            renderer.SetTexture(_textureHandle);
            renderer.SetView(ImitatedGroup.TransformMatrix4X4);
            renderer.SetClipping(ImitatedGroup.UseClipping, Vector2.Zero, Vector2.Zero);

            renderer.DrawBegin(DrawType.Tiles);

            float conv = 1 / 255f;
            float a = _colorChannels[3] * conv;
            float r = _colorChannels[0] * conv * a;
            float g = _colorChannels[1] * conv * a;
            float b = _colorChannels[2] * conv * a;

            renderer.SetColor(r, g, b, a);

            for (int y = 0; y < ImitatedTileLayer.Height; y++)
            {
                for (int x = 0; x < ImitatedTileLayer.Width; x++)
                {
                    int index = x + y * ImitatedTileLayer.Width;
                    var tile = ImitatedTileLayer.Tiles[index];

                    var points = RenderingUtilities.GetTilePos(x, y);
                    var texCoords = RenderingUtilities.GetDefaultTextureCoordinates();

                    renderer.SetTextureCoordinates(
                        texCoords[0],
                        texCoords[1],
                        texCoords[2],
                        texCoords[3], tile.Index);

                    renderer.Draw(points[0], points[1], points[2], points[3]);
                }
            }

            renderer.DrawEnd();

            _graphicsComponent.ExecuteCommandLists();

            _hasUpdateAfterLastDraw = false;
        }

        #endregion
    }
}
