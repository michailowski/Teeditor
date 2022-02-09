using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Teeditor.Common.Models.Bindable;
using Windows.Foundation;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic
{
    internal class QuadsLayerSelection : BindableBase, ILayerSelection
    {
        private Dictionary<MapQuad, List<int>> _container;
        private bool _isEmpty = true;
        private bool _isTransformationAllowed = false;

        public bool IsEmpty
        {
            get => _isEmpty;
            private set => Set(ref _isEmpty, value);
        }

        public bool IsTransformationAllowed
        {
            get => _isTransformationAllowed;
            private set => Set(ref _isTransformationAllowed, value);
        }

        public QuadsLayerSelection()
        {
            _container = new Dictionary<MapQuad, List<int>>();
        }

        public List<MapQuadPoint> GetPoints()
        {
            var result = new List<MapQuadPoint>();

            foreach (var item in _container)
            {
                foreach (int pointId in item.Value)
                {
                    result.Add(item.Key.Points[pointId]);
                }
            }

            return result;
        }

        public List<MapQuad> GetQuadsByCenter()
        {
            var result = new List<MapQuad>();

            foreach (var item in _container)
            {
                foreach (int pointId in item.Value.Where(x => x == 4))
                {
                    result.Add(item.Key);
                }
            }

            return result;
        }

        public bool HasPoint(MapQuad quad, int pointId)
        {
            if (_container.ContainsKey(quad) && _container[quad].Contains(pointId))
                return true;

            return false;
        }

        public bool TryAddPoint(MapQuad quad, int pointId)
        {
            if (pointId == 4)
            {
                IsTransformationAllowed = true;
            }

            if (_container.ContainsKey(quad) && !_container[quad].Contains(pointId))
            {
                _container[quad].Add(pointId);
                return true;
            }
            else if (!_container.ContainsKey(quad))
            {
                _container.Add(quad, new List<int> { pointId });
                return true;
            }

            return false;
        }

        public bool IntersectsRect(Rect sourceRect)
        {
            foreach (var item in _container)
            {
                foreach (int pointId in item.Value)
                {
                    var point = item.Key.Points[pointId];

                    if (sourceRect.Contains(point.Position.ToPoint()))
                        return true;
                }
            }

            return false;
        }

        public void FlushUpdates()
        {
            foreach (var item in _container)
            {
                foreach (var p in item.Key.Points)
                {
                    p.LastPosition = p.Position;
                }
            }
        }

        public void Clear()
        { 
            _container.Clear();
            IsEmpty = true;
            IsTransformationAllowed = false;
        }

        public void FlipHorizontal()
        {
            if (IsTransformationAllowed == false)
                return;

            var quads = GetQuadsByCenter();

            foreach (var quad in quads)
            {
                var center = quad.Points[4].Position;

                Matrix3x2 scale3x2 = Matrix3x2.CreateScale(-1, 1, center);

                for (int i = 0; i < 4; i++)
                {
                    quad.Points[i].Position = Vector2.Transform(quad.Points[i].Position, scale3x2);
                }
            }
        }

        public void FlipVertical()
        {
            if (IsTransformationAllowed == false)
                return;

            var quads = GetQuadsByCenter();

            foreach (var quad in quads)
            {
                var center = quad.Points[4].Position;

                Matrix3x2 scale3x2 = Matrix3x2.CreateScale(1, -1, center);

                for (int i = 0; i < 4; i++)
                {
                    quad.Points[i].Position = Vector2.Transform(quad.Points[i].Position, scale3x2);
                }
            }
        }

        public void Rotate(float rotationAngle)
        {
            if (IsTransformationAllowed == false)
                return;

            var quads = GetQuadsByCenter();
            var amountRadians = rotationAngle * MathF.PI / 180;

            foreach (var quad in quads)
            {
                var center = quad.Points[4].Position;
                var rotate3x2 = Matrix3x2.CreateRotation(amountRadians, center);

                for (int i = 0; i < 4; i++)
                {
                    quad.Points[i].Position = Vector2.Transform(quad.Points[i].Position, rotate3x2);
                }
            }
        }
    }
}
