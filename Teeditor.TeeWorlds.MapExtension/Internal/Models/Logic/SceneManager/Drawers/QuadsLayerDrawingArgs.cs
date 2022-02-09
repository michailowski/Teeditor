using System;
using System.Collections.ObjectModel;
using System.Numerics;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor_TeeWorlds_Direct3DInterop;
using Windows.Foundation;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager.Drawers
{
    internal struct QuadsLayerDrawingArgs
    {
        public MapQuadsLayer Layer { get; set; }
        public Matrix4x4 TransformMatrix4x4 { get; set; }
        public TextureHandle TextureHandle { get; set; }
        public ReadOnlyObservableCollection<MapEnvelope> Envelopes { get; set; }
        public TimeSpan Time { get; set; }
        public bool UseClipping { get; set; }
        public Vector2 ClippingTopLeft { get; set; }
        public Vector2 ClippingBottomRight { get; set; }
    }
}
