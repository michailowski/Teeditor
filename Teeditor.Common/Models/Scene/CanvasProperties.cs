using System.Numerics;

namespace Teeditor.Common.Models.Scene
{
    public static class CanvasProperties
    {
        public static float Width { get; set; }
        public static float Height { get; set; }
        public static Vector2 Center => new Vector2(Width / 2, Height / 2);
        public static Vector2 Size => new Vector2(Width, Height);
    }
}
