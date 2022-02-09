using System;
using System.Numerics;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor_TeeWorlds_Direct3DInterop;
using Windows.Foundation;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager.Drawers
{
    internal struct TilesLayerDrawingArgs
    {
        public MapTilesLayer Layer { get; set; }
        public Matrix4x4 TransformMatrix4x4 { get; set; }
        public TextureHandle TextureHandle { get; set; }
        public MapEnvelope Envelope { get; set; }
        public TimeSpan Time { get; set; }

        public bool UseClipping { get; set; }
        public Vector2 ClippingTopLeft { get; set; }
        public Vector2 ClippingBottomRight { get; set; }

        public int StartX { get; set; }
        public int StartY { get; set; }
        public int EndX { get; set; }
        public int EndY { get; set; }
    }
}
