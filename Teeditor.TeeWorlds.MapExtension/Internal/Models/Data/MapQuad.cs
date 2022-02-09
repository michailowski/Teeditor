using System.Numerics;
using Teeditor.Common;
using System;
using Teeditor.Common.Models;
using Teeditor.TeeWorlds.MapExtension.Internal.Utilities;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data
{
    internal class MapQuad : MapItem
    {
        private int _posEnvIndex = -1;
        private int _posEnvOffset = 0;
        private int _colorEnvIndex = -1;
        private int _colorEnvOffset = 0;

        public int PosEnvIndex
        {
            get => _posEnvIndex;
            set => Set(ref _posEnvIndex, value);
        }
        public int PosEnvOffset
        {
            get => _posEnvOffset;
            set => Set(ref _posEnvOffset, value);
        }
        public int ColorEnvIndex
        {
            get => _colorEnvIndex;
            set => Set(ref _colorEnvIndex, value);
        }
        public int ColorEnvOffset
        {
            get => _colorEnvOffset;
            set => Set(ref _colorEnvOffset, value);
        }
        public MapQuadPoint[] Points { get; set; } = new MapQuadPoint[5];

        public MapQuad()
        {
            for (int i = 0; i < 5; i++)
            {
                Points[i] = new MapQuadPoint();
            }

            Points[0].Position = Points[0].LastPosition = Vector2.Zero;
            Points[1].Position = Points[1].LastPosition = new Vector2(64, 0);
            Points[2].Position = Points[2].LastPosition = new Vector2(0, 64);
            Points[3].Position = Points[3].LastPosition = new Vector2(64, 64);
            Points[4].Position = Points[4].LastPosition = new Vector2(32, 32);

            Points[0].Texture = Vector2.Zero;
            Points[1].Texture = new Vector2(1, 0);
            Points[2].Texture = new Vector2(0, 1);
            Points[3].Texture = new Vector2(1, 1);
            Points[4].Texture = Vector2.Zero;
        }

        public MapQuad(byte[] data) : this()
        {
            int offset = 0;

            for (int l = 0; l < 5; l++)
            {
                offset = l * 8;

                int x = data[offset + 3] << 24 | data[offset + 2] << 16 | data[offset + 1] << 8 | data[offset];
                int y = data[offset + 7] << 24 | data[offset + 6] << 16 | data[offset + 5] << 8 | data[offset + 4];

                Points[l].Position = new Vector2(CalcUtilities.ToFloat(x), CalcUtilities.ToFloat(y));
                Points[l].LastPosition = Points[l].Position;
            }

            for (int l = 0; l < 4; l++)
            {
                offset = 40 + l * 16;

                int r = data[offset + 3] << 24 | data[offset + 2] << 16 | data[offset + 1] << 8 | data[offset];
                int g = data[offset + 7] << 24 | data[offset + 6] << 16 | data[offset + 5] << 8 | data[offset + 4];
                int b = data[offset + 11] << 24 | data[offset + 10] << 16 | data[offset + 9] << 8 | data[offset + 8];
                int a = data[offset + 15] << 24 | data[offset + 14] << 16 | data[offset + 13] << 8 | data[offset + 12];

                Points[l].Color = Windows.UI.Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);//new Vector4(r, g, b, a);
            }

            for (int l = 0; l < 4; l++)
            {
                offset = 104 + l * 8;

                int x = data[offset + 3] << 24 | data[offset + 2] << 16 | data[offset + 1] << 8 | data[offset];
                int y = data[offset + 7] << 24 | data[offset + 6] << 16 | data[offset + 5] << 8 | data[offset + 4];

                Points[l].Texture = new Vector2(CalcUtilities.ToFloat(x), CalcUtilities.ToFloat(y));
            }

            offset = 136;
            PosEnvIndex = data[offset + 3] << 24 | data[offset + 2] << 16 | data[offset + 1] << 8 | data[offset];

            offset = 140;
            PosEnvOffset = data[offset + 3] << 24 | data[offset + 2] << 16 | data[offset + 1] << 8 | data[offset];

            offset = 144;
            ColorEnvIndex = data[offset + 3] << 24 | data[offset + 2] << 16 | data[offset + 1] << 8 | data[offset];

            offset = 148;
            ColorEnvOffset = data[offset + 3] << 24 | data[offset + 2] << 16 | data[offset + 1] << 8 | data[offset];
        }
    }
}
