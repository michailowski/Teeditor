using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teeditor.TeeWorlds.MapExtension.Internal.Extensions;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic
{
    internal static class QuadsPacker
    {
        public static byte[] Pack(ObservableCollection<MapQuad> quads)
        {
            var data = new byte[quads.Count * 152];

            for (int i = 0; i < quads.Count; i++)
            {
                int offset = 0;

                for (int l = 0; l < 5; l++)
                {
                    offset = i * 152 + l * 8;

                    var x = BitConverter.GetBytes(CalcUtilities.ToFixed(quads[i].Points[l].Position.X));
                    var y = BitConverter.GetBytes(CalcUtilities.ToFixed(quads[i].Points[l].Position.Y));

                    data[offset] = x[0]; data[offset + 1] = x[1]; data[offset + 2] = x[2]; data[offset + 3] = x[3];
                    data[offset + 4] = y[0]; data[offset + 5] = y[1]; data[offset + 6] = y[2]; data[offset + 7] = y[3];
                }

                for (int l = 0; l < 4; l++)
                {
                    offset = i * 152 + 40 + l * 16;

                    var r = BitConverter.GetBytes((int)quads[i].Points[l].Color.R);
                    var g = BitConverter.GetBytes((int)quads[i].Points[l].Color.G);
                    var b = BitConverter.GetBytes((int)quads[i].Points[l].Color.B);
                    var a = BitConverter.GetBytes((int)quads[i].Points[l].Color.A);

                    data[offset] = r[0]; data[offset + 1] = r[1]; data[offset + 2] = r[2]; data[offset + 3] = r[3];
                    data[offset + 4] = g[0]; data[offset + 5] = g[1]; data[offset + 6] = g[2]; data[offset + 7] = g[3];
                    data[offset + 8] = b[0]; data[offset + 9] = b[1]; data[offset + 10] = b[2]; data[offset + 11] = b[3];
                    data[offset + 12] = a[0]; data[offset + 13] = a[1]; data[offset + 14] = a[2]; data[offset + 15] = a[3];
                }

                for (int l = 0; l < 4; l++)
                {
                    offset = i * 152 + 104 + l * 8;

                    var x = BitConverter.GetBytes(CalcUtilities.ToFixed(quads[i].Points[l].Texture.X));
                    var y = BitConverter.GetBytes(CalcUtilities.ToFixed(quads[i].Points[l].Texture.Y));

                    data[offset] = x[0]; data[offset + 1] = x[1]; data[offset + 2] = x[2]; data[offset + 3] = x[3];
                    data[offset + 4] = y[0]; data[offset + 5] = y[1]; data[offset + 6] = y[2]; data[offset + 7] = y[3];
                }

                offset = i * 152 + 136;
                var posEnvIndex = BitConverter.GetBytes(quads[i].PosEnvIndex);
                data[offset] = posEnvIndex[0]; data[offset + 1] = posEnvIndex[1]; data[offset + 2] = posEnvIndex[2]; data[offset + 3] = posEnvIndex[3];

                offset = i * 152 + 140;
                var posEnvOffset = BitConverter.GetBytes(quads[i].PosEnvOffset);
                data[offset] = posEnvOffset[0]; data[offset + 1] = posEnvOffset[1]; data[offset + 2] = posEnvOffset[2]; data[offset + 3] = posEnvOffset[3];

                offset = i * 152 + 144;
                var colorEnvIndex = BitConverter.GetBytes(quads[i].ColorEnvIndex);
                data[offset] = colorEnvIndex[0]; data[offset + 1] = colorEnvIndex[1]; data[offset + 2] = colorEnvIndex[2]; data[offset + 3] = colorEnvIndex[3];

                offset = i * 152 + 148;
                var colorEnvOffset = BitConverter.GetBytes(quads[i].ColorEnvOffset);
                data[offset] = colorEnvOffset[0]; data[offset + 1] = colorEnvOffset[1]; data[offset + 2] = colorEnvOffset[2]; data[offset + 3] = colorEnvOffset[3];
            }

            return data;
        }

        public static ObservableCollection<MapQuad> Unpack(byte[] data)
        {
            var quads = new ObservableCollection<MapQuad>();

            if (data == null)
                return quads;

            for (int i = 0; i < data.Length / 152; i++)
            {
                var quadData = data.Skip(i * 152).Take(152).ToArray();
                var quad = new MapQuad(quadData);
                quads.Add(quad);
            }

            return quads;
        }
    }
}
