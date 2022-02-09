using System.Numerics;
using Windows.UI;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data
{
    internal class MapQuadPoint : MapItem
    {
        private Vector2 _position = Vector2.Zero;
        private Vector2 _lastPosition = Vector2.Zero;
        private Color _color = Colors.White;
        private Vector2 _texture = Vector2.Zero;

        public Vector2 Position 
        { 
            get => _position; 
            set => _position = value; 
        }

        public float PositionX
        {
            get => Position.X;
            set
            {
                Position = new Vector2(value, Position.Y);
                OnPropertyChanged();
            }
        }

        public float PositionY
        {
            get => Position.Y;
            set
            {
                Position = new Vector2(Position.X, value);
                OnPropertyChanged();
            }
        }

        public Vector2 LastPosition 
        { 
            get => _lastPosition; 
            set => _lastPosition = value; 
        }

        public Color Color
        {
            get => _color;
            set => Set(ref _color, value);
        }

        public Vector2 Texture 
        { 
            get => _texture; 
            set => _texture = value; 
        }

        public float TextureX
        {
            get => Texture.X;
            set
            {
                Texture = new Vector2(value, Texture.Y);
                OnPropertyChanged();
            }
        }

        public float TextureY
        {
            get => Texture.Y;
            set
            {
                Texture = new Vector2(Texture.X, value);
                OnPropertyChanged();
            }
        }
    }
}
