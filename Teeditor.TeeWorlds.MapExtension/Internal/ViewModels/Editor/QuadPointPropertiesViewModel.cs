using System;
using System.Collections.Generic;
using System.Linq;
using Teeditor.Common.Models.Bindable;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Windows.UI;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Editor
{
    internal class QuadPointPropertiesViewModel : BindableBase
    {
        private int _pointId;
        private MapQuadsLayer _layer;
        private EnvelopesContainer _envelopesContainer;
        private MapEnvelope _colorEnvelope;
        private MapEnvelope _positionEnvelope;

        public List<MapEnvelope> Envelopes
            => _envelopesContainer.Items.ToList();

        public IEnumerable<MapEnvelope> ColorEnvelopes 
            => _envelopesContainer.Items.Where(x => x.Type == Enumerations.EnvelopeType.Color);

        public IEnumerable<MapEnvelope> PositionEnvelopes
            => _envelopesContainer.Items.Where(x => x.Type == Enumerations.EnvelopeType.Position);

        public MapEnvelope ColorEnvelope 
        {
            get => _colorEnvelope;
            set
            {
                Quad.ColorEnvIndex = Envelopes.IndexOf(value);
                Set(ref _colorEnvelope, value);
            }
        }

        public MapEnvelope PositionEnvelope
        {
            get => _positionEnvelope;
            set
            {
                Quad.PosEnvIndex = Envelopes.IndexOf(value);
                Set(ref _positionEnvelope, value);
            }
        }

        public MapQuad Quad { get; }

        public MapQuadPoint Point => Quad.Points[_pointId];

        public bool IsAspectRatioAvailable => _layer?.Image != null;

        public bool IsCenterPoint => _pointId == 4;

        public bool IsPerimeterPoint => _pointId < 4;

        public event EventHandler QuadRemoved;

        public QuadPointPropertiesViewModel(int pointId, MapQuad quad, MapQuadsLayer layer, EnvelopesContainer envelopesContainer)
        {
            _pointId = pointId;
            _layer = layer;
            _envelopesContainer = envelopesContainer;

            Quad = quad;

            _colorEnvelope = Quad.ColorEnvIndex >= 0 ? Envelopes[Quad.ColorEnvIndex] : null;
            _positionEnvelope = Quad.PosEnvIndex >= 0 ? Envelopes[Quad.PosEnvIndex] : null;
        }

        public void AspectRatio()
        {
            int top = (int)Quad.Points[0].PositionY;
            int left = (int)Quad.Points[0].PositionX;
            int right = (int)Quad.Points[0].PositionX;

            for (int k = 1; k < 4; k++)
            {
                if (Quad.Points[k].PositionY < top) top = (int)Quad.Points[k].PositionY;
                if (Quad.Points[k].PositionX < left) left = (int)Quad.Points[k].PositionX;
                if (Quad.Points[k].PositionX > right) right = (int)Quad.Points[k].PositionX;
            }

            int height = (right - left) * _layer.Image.Height / _layer.Image.Width;

            Quad.Points[0].PositionX = left; Quad.Points[0].PositionY = top;
            Quad.Points[1].PositionX = right; Quad.Points[1].PositionY = top;
            Quad.Points[2].PositionX = left; Quad.Points[2].PositionY = top + height;
            Quad.Points[3].PositionX = right; Quad.Points[3].PositionY = top + height;

            Quad.Points[0].LastPosition = Quad.Points[0].Position;
            Quad.Points[1].LastPosition = Quad.Points[1].Position;
            Quad.Points[2].LastPosition = Quad.Points[2].Position;
            Quad.Points[3].LastPosition = Quad.Points[3].Position;
        }

        public void SquareQuad()
        {
            int top = (int)Quad.Points[0].PositionY;
            int left = (int)Quad.Points[0].PositionX;
            int bottom = (int)Quad.Points[0].PositionY;
            int right = (int)Quad.Points[0].PositionX;

            for (int k = 1; k < 4; k++)
            {
                if (Quad.Points[k].PositionY < top) top = (int)Quad.Points[k].PositionY;
                if (Quad.Points[k].PositionX < left) left = (int)Quad.Points[k].PositionX;
                if (Quad.Points[k].PositionY > bottom) bottom = (int)Quad.Points[k].PositionY;
                if (Quad.Points[k].PositionX > right) right = (int)Quad.Points[k].PositionX;
            }

            Quad.Points[0].PositionX = left; Quad.Points[0].PositionY = top;
            Quad.Points[1].PositionX = right; Quad.Points[1].PositionY = top;
            Quad.Points[2].PositionX = left; Quad.Points[2].PositionY = bottom;
            Quad.Points[3].PositionX = right; Quad.Points[3].PositionY = bottom;

            Quad.Points[0].LastPosition = Quad.Points[0].Position;
            Quad.Points[1].LastPosition = Quad.Points[1].Position;
            Quad.Points[2].LastPosition = Quad.Points[2].Position;
            Quad.Points[3].LastPosition = Quad.Points[3].Position;
        }

        public void CenterPivot()
        {
            int top = (int)Quad.Points[0].PositionY;
            int left = (int)Quad.Points[0].PositionX;
            int bottom = (int)Quad.Points[0].PositionY;
            int right = (int)Quad.Points[0].PositionX;

            for (int k = 1; k < 4; k++)
            {
                if (Quad.Points[k].PositionY < top) top = (int)Quad.Points[k].PositionY;
                if (Quad.Points[k].PositionX < left) left = (int)Quad.Points[k].PositionX;
                if (Quad.Points[k].PositionY > bottom) bottom = (int)Quad.Points[k].PositionY;
                if (Quad.Points[k].PositionX > right) right = (int)Quad.Points[k].PositionX;
            }

            Quad.Points[4].PositionX = left + (right - left) / 2;
            Quad.Points[4].PositionY = top + (bottom - top) / 2;

            Quad.Points[4].LastPosition = Quad.Points[4].Position;
        }

        public void RemoveQuad()
        {
            _layer.Quads.Remove(Quad);
            QuadRemoved?.Invoke(this, EventArgs.Empty);
        }

        public void ResetColorEnvelope() => ColorEnvelope = null;

        public void ResetPositionEnvelope() => PositionEnvelope = null;

        public void SetColor(Color color) => Point.Color = color;

        public void ResetColor() => Point.Color = Colors.White;
    }
}
