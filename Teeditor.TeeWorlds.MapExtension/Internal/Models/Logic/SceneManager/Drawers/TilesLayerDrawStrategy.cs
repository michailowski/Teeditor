using System.Threading.Tasks;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;
using Teeditor_TeeWorlds_Direct3DInterop;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager.Drawers
{
    internal class TilesLayerDrawStrategy : ILayerDrawStrategy
    {
        private MapTilesLayer _tilesLayer;

        public void SetLayer(MapTilesLayer tilesLayer) => _tilesLayer = tilesLayer;

        public bool TryGetDrawingTask(out Task drawingTask, DrawingTaskArgs args)
        {
            drawingTask = null;

            var textureHandle = _tilesLayer.Image?.TextureHandle;

            if (textureHandle == null || args.Map.ImagesContainer.TexturesManager.IsTextureArrayLoaded(textureHandle) == false)
                return false;

            var defferedRenderer = args.GraphicsComponent.GetDefferedRenderer();

            var drawingArgs = new TilesLayerDrawingArgs()
            {
                Layer = _tilesLayer,
                TransformMatrix4x4 = args.TransformMatrix4x4,
                TextureHandle = textureHandle,
                Envelope = _tilesLayer.ColorEnvelope,
                Time = args.Time,
                UseClipping = args.UseClipping,
                ClippingTopLeft = args.ClippingTopLeft,
                ClippingBottomRight = args.ClippingBottomRight,
                StartX = (int)(args.StartPos.X / RenderingUtilities.GridUnitSize) - 1,
                StartY = (int)(args.StartPos.Y / RenderingUtilities.GridUnitSize) - 1,
                EndX = (int)(args.EndPos.X / RenderingUtilities.GridUnitSize) + 1,
                EndY = (int)(args.EndPos.Y / RenderingUtilities.GridUnitSize) + 1
            };

            drawingTask = Task.Factory.StartNew(() => DrawLayer(defferedRenderer, drawingArgs));

            return true;
        }

        private void DrawLayer(DefferedRenderer defferedRenderer, TilesLayerDrawingArgs args)
        {
            defferedRenderer.SetTexture(args.TextureHandle);
            defferedRenderer.SetView(args.TransformMatrix4x4);
            defferedRenderer.SetClipping(args.UseClipping, args.ClippingTopLeft, args.ClippingBottomRight);

            defferedRenderer.DrawBegin(DrawType.Tiles);

            float r = 1, g = 1, b = 1, a = 1;

            if (args.Envelope != null)
            {
                var time = (float)args.Time.TotalSeconds + args.Layer.ColorEnvelopeOffset / 1000.0f;
                args.Envelope.TryEvaluateColor(time, out r, out g, out b, out a);
            }
            else
            {
                a = args.Layer.ColorChannels[3] / 255f;
                r = args.Layer.ColorChannels[0] / 255f * a;
                g = args.Layer.ColorChannels[1] / 255f * a;
                b = args.Layer.ColorChannels[2] / 255f * a;
            }

            defferedRenderer.SetColor(r, g, b, a);

            for (int y = args.StartY; y < args.EndY; y++)
            {
                for (int x = args.StartX; x < args.EndX; x++)
                {
                    if (x < 0 || y < 0 || x >= args.Layer.Width || y >= args.Layer.Height)
                        continue;

                    var tile = args.Layer.Tiles[x + y * args.Layer.Width];

                    if (tile.Index != 0)
                    {
                        var points = RenderingUtilities.GetTilePos(x, y);
                        var texCoords = RenderingUtilities.GetDefaultTextureCoordinates();

                        if (tile.IsVerticallyFlipped && !args.Layer.IsGameLayer)
                        {
                            RenderingUtilities.FlipTextureCoordinatesVertically(ref texCoords);
                        }

                        if (tile.IsHorizontallyFlipped && !args.Layer.IsGameLayer)
                        {
                            RenderingUtilities.FlipTextureCoordinatesHorizontally(ref texCoords);
                        }

                        if (tile.IsRotated && !args.Layer.IsGameLayer)
                        {
                            RenderingUtilities.RotateTextureCoordinates(ref texCoords);
                        }

                        defferedRenderer.SetTextureCoordinates(
                            texCoords[0],
                            texCoords[1],
                            texCoords[2],
                            texCoords[3], tile.Index);

                        defferedRenderer.Draw(points[0], points[1], points[2], points[3]);
                    }
                }
            }

            defferedRenderer.DrawEnd();
        }
    }
}
