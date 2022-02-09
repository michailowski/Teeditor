using System;
using System.Numerics;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor_TeeWorlds_Direct3DInterop;
using Windows.Foundation;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager.Drawers
{
    internal struct DrawingTaskArgs
    {
        public GraphicsComponent GraphicsComponent { get; set; }
        public Matrix4x4 TransformMatrix4x4 { get; set; }
        public Vector2 StartPos { get; set; }
        public Vector2 EndPos { get; set; }
        public TimeSpan Time { get; set; }
        public bool UseClipping { get; set; }
        public Vector2 ClippingTopLeft { get; set; }
        public Vector2 ClippingBottomRight { get; set; }
        public Map Map { get; set; }
    }
}
