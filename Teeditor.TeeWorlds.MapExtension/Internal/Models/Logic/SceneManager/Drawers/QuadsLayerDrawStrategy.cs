using System.Numerics;
using System.Threading.Tasks;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;
using Teeditor_TeeWorlds_Direct3DInterop;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager.Drawers
{
    internal class QuadsLayerDrawStrategy : ILayerDrawStrategy
    {
        private MapQuadsLayer _quadsLayer;

        public void SetLayer(MapQuadsLayer quadsLayer) =>  _quadsLayer = quadsLayer;
        
        public bool TryGetDrawingTask(out Task drawingTask, DrawingTaskArgs args)
        {
            var defferedRenderer = args.GraphicsComponent.GetDefferedRenderer();

            var drawingArgs = new QuadsLayerDrawingArgs()
            {
                Layer = _quadsLayer,
                TransformMatrix4x4 = args.TransformMatrix4x4,
                TextureHandle = _quadsLayer?.Image?.TextureHandle,
                Envelopes = args.Map.EnvelopesContainer.Items,
                Time = args.Time,
                UseClipping = args.UseClipping,
                ClippingTopLeft = args.ClippingTopLeft,
                ClippingBottomRight = args.ClippingBottomRight
            };

            drawingTask = Task.Factory.StartNew(() => DrawLayer(defferedRenderer, drawingArgs));

            return true;
        }

        private void DrawLayer(DefferedRenderer defferedRenderer, QuadsLayerDrawingArgs args)
        {
            defferedRenderer.SetTexture(args.TextureHandle);
            defferedRenderer.SetView(args.TransformMatrix4x4);
            defferedRenderer.SetClipping(args.UseClipping, args.ClippingTopLeft, args.ClippingBottomRight);

            defferedRenderer.DrawBegin(DrawType.Quads);

            for (int i = 0; i < args.Layer.Quads.Count; i++)
            {
                float r = 1, g = 1, b = 1, a = 1;
                var offset = Vector2.Zero;
                var rotate = 0f;

                var colors = new Vector4[args.Layer.Quads[i].Points.Length];
                var points = new Vector2[args.Layer.Quads[i].Points.Length];

                if (args.Layer.Quads[i].ColorEnvIndex >= 0 && args.Layer.Quads[i].ColorEnvIndex < args.Envelopes.Count)
                {
                    var time = (float)args.Time.TotalSeconds + args.Layer.Quads[i].ColorEnvOffset / 1000.0f;
                    args.Envelopes[args.Layer.Quads[i].ColorEnvIndex].TryEvaluateColor(time, out r, out g, out b, out a);
                }

                if (args.Layer.Quads[i].PosEnvIndex >= 0 && args.Layer.Quads[i].PosEnvIndex < args.Envelopes.Count)
                {
                    var time = (float)args.Time.TotalSeconds + args.Layer.Quads[i].PosEnvOffset / 1000.0f;
                    args.Envelopes[args.Layer.Quads[i].PosEnvIndex].TryEvaluatePosition(time, out offset, out rotate);
                }

                for (int j = 0; j < args.Layer.Quads[i].Points.Length; j++)
                {
                    colors[j] = args.Layer.Quads[i].Points[j].Color.Multiply(r, g, b, a);
                    points[j] = args.Layer.Quads[i].Points[j].Position;
                    points[j] += offset;
                }

                if (rotate != 0)
                {
                    RenderingUtilities.RotatePoints(args.Layer.Quads[i].Points[4].Position, ref points, rotate);
                }

                defferedRenderer.SetColor4(colors);

                defferedRenderer.SetTextureCoordinates(
                    args.Layer.Quads[i].Points[0].Texture,
                    args.Layer.Quads[i].Points[1].Texture,
                    args.Layer.Quads[i].Points[2].Texture,
                    args.Layer.Quads[i].Points[3].Texture, -1);

                defferedRenderer.Draw(points[0], points[1], points[2], points[3]);
            }

            defferedRenderer.DrawEnd();
        }
    }
}
