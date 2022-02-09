using System;
using System.Numerics;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using Microsoft.Graphics.Canvas.Brushes;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.ComponentModel;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using Teeditor.Common.ViewModels;
using Teeditor.Common.Utilities;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Models.Sidebar;
using Teeditor.Common.Models.Scene;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar
{
    internal class NavigatorBoxViewModel : BoxViewModelBase
    {
        private Map _map;

        private SceneCamera _camera;

        private float _ratio = 0f;
        private float _gridUnitRatio = 0.2f;
        private Vector2 _contentBox = Vector2.Zero;
        private Size _mapScaledSizeInPixels;
        private Vector2 _lastTouchedPos;

        public Rect ViewBox { get; private set; }

        public float ZoomLevel => _camera.ZoomLevel;

        public event EventHandler MinimapUpdated;
        public event EventHandler ViewboxUpdated;

        public event ZoomLevelChangeHandler ZoomLevelChanged;
        public delegate void ZoomLevelChangeHandler(object sender, float value);

        public NavigatorBoxViewModel()
        {
            Label = "Navigator";
            MenuText = "Navigator Box";
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["NavigatorBoxIconPath"]) };
            Dock = SidebarDock.Right;
        }

        public override void SetTab(ITab tab)
        {
            if (_map != null)
            {
                _map.VisualChanged -= Map_VisualChanged;
                _map = null;
            }

            if (_camera != null)
            {
                _camera.PropertyChanged -= Camera_PropertyChanged;
                _camera.PositionChanged -= Camera_PositionChanged;
                _camera = null;
            }

            if (tab == null)
                return;

            _map = tab.Data as Map;

            var sceneManager = (MapSceneManager) tab.SceneManager;
            _camera = sceneManager.Camera;

            UpdateMinimap();
            UpdateViewBox();

            _camera.PropertyChanged += Camera_PropertyChanged;
            _camera.PositionChanged += Camera_PositionChanged;
            _map.VisualChanged += Map_VisualChanged;

            // DynamicViewModelBase need this setting for using of binding to model with dynamic properties
            DynamicModel = _camera;
        }

        public void UpdateMinimap()
            => MinimapUpdated?.Invoke(this, EventArgs.Empty);

        public void UpdateViewBox()
        {
            var translationVector2 = -_camera.Position * _gridUnitRatio;
            var translationToCenter3x2 = Matrix3x2.CreateTranslation(CanvasProperties.Center);
            var translation3x2 = Matrix3x2.CreateTranslation(translationVector2);
            var scale3x2 = Matrix3x2.CreateScale(_camera.Scale / _gridUnitRatio, CanvasProperties.Center);

            var worldMatrix = translation3x2 * translationToCenter3x2 * scale3x2;

            RenderingUtilities.ScreenToWorld(worldMatrix, out var wStartPos, out var wEndPos);

            var rect = new Rect(
                _contentBox.X + wStartPos.X / _ratio,
                _contentBox.Y + wStartPos.Y / _ratio,
                (wEndPos.X - wStartPos.X) / _ratio,
                (wEndPos.Y - wStartPos.Y) / _ratio
            );

            rect = new Rect(
                new Point(
                    Math.Max(rect.X, _contentBox.X),
                    Math.Max(rect.Y, _contentBox.Y)),
                new Point(
                    Math.Min(rect.X + rect.Width, _contentBox.X + _mapScaledSizeInPixels.Width),
                    Math.Min(rect.Y + rect.Height, _contentBox.Y + _mapScaledSizeInPixels.Height))
            );

            if (rect.X + rect.Width > _contentBox.X && rect.Y + rect.Height > _contentBox.Y &&
                rect.X < _contentBox.X + _mapScaledSizeInPixels.Width && rect.Y < _contentBox.Y + _mapScaledSizeInPixels.Height)
            {
                ViewBox = rect;
                ViewboxUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public void CalcParameters(float canvasWidth, float canvasHeight)
        {
            _contentBox = Vector2.Zero;

            var mapSizeInPixels = new Size(
                _map.Width * RenderingUtilities.GridUnitSize * _gridUnitRatio,
                _map.Height * RenderingUtilities.GridUnitSize * _gridUnitRatio);

            _ratio = (float)(_map.Width > _map.Height ?
                mapSizeInPixels.Width / canvasWidth :
                mapSizeInPixels.Height / canvasHeight);

            _mapScaledSizeInPixels = new Size(
                mapSizeInPixels.Width / _ratio,
                mapSizeInPixels.Height / _ratio);

            if (_map.Width > _map.Height)
                _contentBox.Y = ((float)canvasHeight - (float)_mapScaledSizeInPixels.Height) / 2;
            else
                _contentBox.X = ((float)canvasWidth - (float)_mapScaledSizeInPixels.Width) / 2;
        }

        public void DrawChecksBackground(CanvasDrawingSession cds, float width, float height)
        {
            Color firstCheck = Color.FromArgb(255, 153, 153, 153);
            Color secondCheck = Color.FromArgb(255, 186, 186, 186);

            var checks = CanvasBitmap.CreateFromColors(cds.Device, new Color[] { firstCheck, secondCheck, secondCheck, firstCheck }, 2, 2);
            var fillPatternBrush = new CanvasImageBrush(cds.Device, checks)
            {
                ExtendX = CanvasEdgeBehavior.Wrap,
                ExtendY = CanvasEdgeBehavior.Wrap,
                Transform = Matrix3x2.CreateScale(8),
                Interpolation = CanvasImageInterpolation.NearestNeighbor
            };

            cds.FillRectangle(0, 0, width, height, fillPatternBrush);
        }

        public void DrawMinimap(CanvasDrawingSession cds)
        {
            MapTilesLayer gameLayer = null;
            MapGroup gameGroup = null;

            var groups = _map.GroupedLayersContainer.Groups;

            for (int i = 0; i < groups.Count; i++)
            {
                for (int j = 0; j < groups[i].Layers.Count; j++)
                {
                    if (groups[i].Layers[j] is MapTilesLayer layerTiles)
                    {
                        if (layerTiles.IsGameLayer)
                        {
                            gameLayer = layerTiles;
                            gameGroup = groups[i];
                        }
                        else
                        {
                            if (layerTiles.IsHighDetail)
                                continue;

                            DrawLayerThumbnail(cds, layerTiles, groups[i]);
                        }
                    }
                }
            }

            if (gameLayer == null || gameGroup == null)
                return;

            DrawLayerThumbnail(cds, gameLayer, gameGroup);
        }

        public void DrawLayerThumbnail(CanvasDrawingSession cds, MapTilesLayer layer, MapGroup group)
        {
            if (!layer.HasThumbnail)
                return;

            var sourceRect = new Rect(
                @group.Offset.X * 2 * _gridUnitRatio,
                @group.Offset.Y * 2 * _gridUnitRatio,
                layer.Width * RenderingUtilities.GridUnitSize * _gridUnitRatio,
                layer.Height * RenderingUtilities.GridUnitSize * _gridUnitRatio);

            var destRect = new Rect(
                _contentBox.X,
                _contentBox.Y,
                sourceRect.Width / _ratio,
                sourceRect.Height / _ratio);

            cds.DrawImage(layer.Thumbnail, destRect, sourceRect);
        }

        public void ViewBoxBeingDrag(Vector2 currentPointerPos)
        {
            var deltaPos = new Vector2(_lastTouchedPos.X - currentPointerPos.X,
                _lastTouchedPos.Y - currentPointerPos.Y) * _ratio / _gridUnitRatio;

            var newPos = _camera.LastReleasePosition - deltaPos;

            var left = CanvasProperties.Center.X / _camera.Scale;
            var right = _map.Width * RenderingUtilities.GridUnitSize - left;
            var top = CanvasProperties.Center.Y / _camera.Scale;
            var bottom = _map.Height * RenderingUtilities.GridUnitSize - top;

            var x = Math.Min(newPos.X, right);
            x = Math.Max(x, left);

            var y = Math.Min(newPos.Y, bottom);
            y = Math.Max(y, top);

            if (_camera.Position.X < left && _camera.Position.X + CanvasProperties.Width > right &&
                _camera.Position.Y < top && _camera.Position.Y + CanvasProperties.Height > bottom)
                return;

            _camera.Position = new Vector2(x, y);
        }

        public void ViewBoxStartDrag(Vector2 startPointerPos)
            => _lastTouchedPos = startPointerPos;

        public void ViewBoxEndDrag()
            => _camera.LastReleasePosition = _camera.Position;

        private async void Camera_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ZoomLevel")
            {
                UpdateViewBox();

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ZoomLevelChanged?.Invoke(this, _camera.ZoomLevel);
                });
            }
        }

        private async void Map_VisualChanged(object sender, VisualChangedEventArgs e)
            => await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, UpdateMinimap);

        private void Camera_PositionChanged(object sender, EventArgs e) => UpdateViewBox();

        public void TryDecreaseZoomLevel() => _camera?.TryDecreaseZoomLevel();

        public void TryIncreaseZoomLevel() => _camera?.TryIncreaseZoomLevel();

        public void SetZoomLevel(float value) => _camera?.SetZoomLevel(value);
    }
}
