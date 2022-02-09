using System;
using System.Numerics;
using Teeditor.Common.Models.Scene;
using Windows.UI;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Utilities
{
    internal static class RenderingUtilities
    {
        public static byte GridUnitSize { get; } = 32;

        public static void CalcProofPoints(float aspect, out Vector2 firstPoint, out Vector2 secondPoint)
        {
            float width, height;

            float amount = 1150 * 1000;
            float maxWidth = 1500;
            float maxHeight = 1050;

            CalcScreenParams(amount, maxWidth, maxHeight, aspect, out width, out height);

            firstPoint.X = CanvasProperties.Center.X - width / 2;
            firstPoint.Y = CanvasProperties.Center.Y - height / 2;
            secondPoint.X = firstPoint.X + width;
            secondPoint.Y = firstPoint.Y + height;
        }

        private static void CalcScreenParams(float amount, float maxWidth, float maxHeight, float aspect, out float width, out float height)
        {
            float f = MathF.Sqrt(amount) / MathF.Sqrt(aspect);
            width = f * aspect;
            height = f;

            if (width > maxWidth)
            {
                width = maxWidth;
                height = width / aspect;
            }

            if (height > maxHeight)
            {
                height = maxHeight;
                width = height * aspect;
            }
        }

        public static void ScreenToWorld(Matrix3x2 matrix, out Vector2 worldTopLeftPos, out Vector2 worldBottomRightPos)
        {
            Matrix3x2 invertedMatrix;

            if (!Matrix3x2.Invert(matrix, out invertedMatrix))
            {
                worldTopLeftPos = worldBottomRightPos = Vector2.Zero;
                return;
            }

            worldTopLeftPos = Vector2.Transform(Vector2.Zero, invertedMatrix);
            worldBottomRightPos = Vector2.Transform(CanvasProperties.Size, invertedMatrix);
        }

        public static void PosToWorld(Matrix3x2 matrix, Vector2 sourcePos, out Vector2 worldPos)
        {
            Matrix3x2 invertedMatrix;

            if (!Matrix3x2.Invert(matrix, out invertedMatrix))
            {
                worldPos = Vector2.Zero;
                return;
            }

            worldPos = Vector2.Transform(sourcePos, invertedMatrix);
        }

        public static void WorldToPos(Matrix3x2 matrix, Vector2 worldPos, out Vector2 sourcePos)
            => sourcePos = Vector2.Transform(worldPos, matrix);

        public static Vector2[] GetTilePos(int x, int y)
        {
            Vector2[] points = new Vector2[4];

            points[0].X = x * GridUnitSize;
            points[0].Y = y * GridUnitSize + GridUnitSize;

            points[1].X = x * GridUnitSize;
            points[1].Y = y * GridUnitSize;

            points[2].X = x * GridUnitSize + GridUnitSize;
            points[2].Y = y * GridUnitSize + GridUnitSize;

            points[3].X = x * GridUnitSize + GridUnitSize;
            points[3].Y = y * GridUnitSize;

            return points;
        }

        public static Vector2[] GetDefaultTextureCoordinates()
        {
            Vector2[] texCoords = new Vector2[4];

            texCoords[0].X = 0;
            texCoords[0].Y = 1;
            texCoords[1].X = 0;
            texCoords[1].Y = 0;
            texCoords[2].X = 1;
            texCoords[2].Y = 1;
            texCoords[3].X = 1;
            texCoords[3].Y = 0;

            return texCoords;
        }

        public static void FlipTextureCoordinatesVertically(ref Vector2[] texCoords)
        {
            texCoords[0].X = texCoords[2].X;
            texCoords[2].X = texCoords[1].X;
            texCoords[1].X = texCoords[3].X;
            texCoords[3].X = texCoords[2].X;
        }

        public static void FlipTextureCoordinatesHorizontally(ref Vector2[] texCoords)
        {
            texCoords[0].Y = texCoords[1].Y;
            texCoords[1].Y = texCoords[2].Y;
            texCoords[2].Y = texCoords[0].Y;
            texCoords[3].Y = texCoords[1].Y;
        }

        public static void RotateTextureCoordinates(ref Vector2[] texCoords)
        {
            float tmp = texCoords[1].X;
            texCoords[1].X = texCoords[0].X;
            texCoords[0].X = texCoords[2].X;
            texCoords[2].X = texCoords[3].X;
            texCoords[3].X = tmp;
            tmp = texCoords[1].Y;
            texCoords[1].Y = texCoords[0].Y;
            texCoords[0].Y = texCoords[2].Y;
            texCoords[2].Y = texCoords[3].Y;
            texCoords[3].Y = tmp;
        }

        public static void RotatePoints(Vector2 center, ref Vector2[] points, float rotation)
        {
            for (int i = 0; i < 4; i++)
            {
                float x = points[i].X - center.X;
                float y = points[i].Y - center.Y;
                points[i].X = x * MathF.Cos(rotation) - y * MathF.Sin(rotation) + center.X;
                points[i].Y = x * MathF.Sin(rotation) + y * MathF.Cos(rotation) + center.Y;
            }
        }

        public static Vector4 Multiply(this Color color, float R, float G, float B, float A)
        {
            float conv = 1 / 255f;
            return new Vector4(color.R * conv * R * color.A * conv * A,
                                       color.G * conv * G * color.A * conv * A,
                                       color.B * conv * B * color.A * conv * A,
                                       color.A * conv * A);
        }

        public static int[] ToArray(this Color color)
            => new int[] { color.R, color.G, color.B, color.A };
    }
}
