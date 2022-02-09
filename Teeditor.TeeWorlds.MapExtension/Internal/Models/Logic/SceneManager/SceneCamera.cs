using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Teeditor.Common.Utilities;
using Teeditor_Direct3DInterop;
using Teeditor_Direct3DInterop.Enumerations;
using Teeditor.Common.Models.Components;
using Teeditor.Common.Models.Scene;
using Teeditor.Common.Models.Bindable;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager
{
    internal class SceneCamera : BindableBase, IMouseInteractionItem
    {
        private Vector2 _position = Vector2.Zero;
        private float _zoomLevel = 100f;
        private bool _isDrag;

        private const float ZoomLevelMin = 10f;
        private const float ZoomLevelMax = 200f;
        private const float ZoomFactor = 2.5f;
        private const float MoveSpeed = 1.7f;

        public Vector2 Position
        {
            get => _position;
            set => SetPosition(value);
        }

        public float ZoomLevel
        {
            get => _zoomLevel;
            private set => Set(ref _zoomLevel, value);
        }

        public float Scale { get; set; } = 1f;
        public Vector2 LastReleasePosition { get; set; }
        public Vector2 LastPressPosition { get; set; }
        public Dictionary<Vector3, string> ViewBookmarks { get; } = new Dictionary<Vector3, string>();

        public event EventHandler PositionChanged;

        private async void SetPosition(Vector2 position)
        {
            _position = position;

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, 
                () => PositionChanged?.Invoke(this, EventArgs.Empty));
        }

        public async void SetZoomLevel(float zoomLevel)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ZoomLevel = Math.Clamp(zoomLevel, ZoomLevelMin, ZoomLevelMax);
                Scale = ZoomLevel / 100f;
            });
        }

        public void TryIncreaseZoomLevel()
        {
            if (ZoomLevel < ZoomLevelMax)
            {
                SetZoomLevel(ZoomLevel + ZoomFactor);
            }
        }

        public void TryDecreaseZoomLevel()
        {
            if (ZoomLevel > ZoomLevelMin)
            {
                SetZoomLevel(ZoomLevel - ZoomFactor);
            }
        }

        public async void TryIncreaseZoomLevelTarget(Vector2 position)
        {
            if (ZoomLevel >= ZoomLevelMax)
                return;

            var translationToCenter3x2 = Matrix3x2.CreateTranslation(CanvasProperties.Center);
            var translation3x2 = Matrix3x2.CreateTranslation(Position);
            var scale3x2 = Matrix3x2.CreateScale(Scale, CanvasProperties.Center);
            var transformMatrix3X2 = translation3x2 * translationToCenter3x2 * scale3x2;

            RenderingUtilities.PosToWorld(transformMatrix3X2, CanvasProperties.Center, out var oldCenterPos);
            RenderingUtilities.PosToWorld(transformMatrix3X2, position, out var oldMousePos);

            var oldDelta = oldMousePos - oldCenterPos;

            var zoomLevel = _zoomLevel + ZoomFactor;
            _zoomLevel = Math.Clamp(zoomLevel, ZoomLevelMin, ZoomLevelMax);
            Scale = _zoomLevel / 100f;

            scale3x2 = Matrix3x2.CreateScale(Scale, CanvasProperties.Center);
            transformMatrix3X2 = translation3x2 * translationToCenter3x2 * scale3x2;

            RenderingUtilities.PosToWorld(transformMatrix3X2, CanvasProperties.Center, out var newCenterPos);
            RenderingUtilities.PosToWorld(transformMatrix3X2, position, out var newMousePos);

            var newDelta = newMousePos - newCenterPos;

            Position += oldDelta - newDelta;
            LastReleasePosition = Position;

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged("ZoomLevel");
            });
        }

        public async void TryDecreaseZoomLevelTarget(Vector2 position)
        {
            if (ZoomLevel <= ZoomLevelMin)
                return;

            var translationToCenter3x2 = Matrix3x2.CreateTranslation(CanvasProperties.Center);
            var translation3x2 = Matrix3x2.CreateTranslation(Position);
            var scale3x2 = Matrix3x2.CreateScale(Scale, CanvasProperties.Center);
            var transformMatrix3X2 = translation3x2 * translationToCenter3x2 * scale3x2;

            RenderingUtilities.PosToWorld(transformMatrix3X2, CanvasProperties.Center, out var oldCenterPos);
            RenderingUtilities.PosToWorld(transformMatrix3X2, position, out var oldMousePos);

            var oldDelta = oldMousePos - oldCenterPos;

            var zoomLevel = _zoomLevel - ZoomFactor;
            _zoomLevel = Math.Clamp(zoomLevel, ZoomLevelMin, ZoomLevelMax);
            Scale = _zoomLevel / 100f;

            scale3x2 = Matrix3x2.CreateScale(Scale, CanvasProperties.Center);
            transformMatrix3X2 = translation3x2 * translationToCenter3x2 * scale3x2;

            RenderingUtilities.PosToWorld(transformMatrix3X2, CanvasProperties.Center, out var newCenterPos);
            RenderingUtilities.PosToWorld(transformMatrix3X2, position, out var newMousePos);

            var newDelta = newMousePos - newCenterPos;

            Position += oldDelta - newDelta;
            LastReleasePosition = Position;

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged("ZoomLevel");
            });
        }

        public bool TryAddCurrentViewBookmark(string name)
        {
            var position = new Vector3(Position.X, Position.Y, ZoomLevel);

            return ViewBookmarks.TryAdd(position, name);
        }

        public void GoToView(Vector3 position)
        {
            Position = new Vector2(position.X, position.Y);
            LastPressPosition = Position;
            LastReleasePosition = Position;

            SetZoomLevel(position.Z);
        }

        public void ResetPosition()
            => Position = LastPressPosition = LastReleasePosition = Vector2.Zero;

        public void ResetZoom()
            => SetZoomLevel(100);

        public void ProcessMouseInput(MouseInput input, out bool handled)
        {
            handled = false;

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

                case MouseInputType.WheelUp:
                    TryIncreaseZoomLevelTarget(input.Position);
                    break;

                case MouseInputType.WheelDown:
                    TryDecreaseZoomLevelTarget(input.Position);
                    break;
            }
        }

        private void OnMouseMove(MouseInput input, out bool handled)
        {
            handled = false;

            bool ctrlModifierPressed = (input.KeyModifiers & MouseKeyModifiers.Control) == MouseKeyModifiers.Control;
            bool leftModifierPressed = (input.KeyModifiers & MouseKeyModifiers.Left) == MouseKeyModifiers.Left;
            bool middleModifierPressed = (input.KeyModifiers & MouseKeyModifiers.Middle) == MouseKeyModifiers.Middle;

            if (leftModifierPressed && ctrlModifierPressed && _isDrag || middleModifierPressed && _isDrag)
            {
                Position = LastReleasePosition + (LastPressPosition - input.Position) / Scale * MoveSpeed;
                handled = true;
            }
        }

        private void OnMousePressed(MouseInput input, out bool handled)
        {
            handled = false;

            bool ctrlModifierPressed = (input.KeyModifiers & MouseKeyModifiers.Control) == MouseKeyModifiers.Control;

            if (input.Button == MouseButton.Middle || input.Button == MouseButton.Left && ctrlModifierPressed)
            {
                UserInterface.SetCursorType(CoreCursorType.SizeAll);
                _isDrag = true;
                LastPressPosition = input.Position;
                handled = true;
            }
        }

        private void OnMouseReleased(MouseInput input, out bool handled)
        {
            handled = false;

            if (input.Button == MouseButton.Middle && _isDrag || input.Button == MouseButton.Left && _isDrag)
            {
                UserInterface.SetCursorType(CoreCursorType.Arrow);
                _isDrag = false;
                LastReleasePosition = Position;
                handled = true;
            }
        }
    }
}
