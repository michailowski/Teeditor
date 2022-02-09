using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Windows.Foundation;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.Panelbar.Animation
{
    internal struct EnvelopeSegment
    {
        public MapEnvelope Envelope { get; }
        public MapEnvelopePoint LeftPoint { get; }
        public MapEnvelopePoint RightPoint { get; }
        public bool IsEmpty => LeftPoint == null || RightPoint == null;

        public Rect Bounds { get; }

        public static EnvelopeSegment Empty => new EnvelopeSegment(null, null, null, Rect.Empty);

        public EnvelopeSegment(MapEnvelope envelope, MapEnvelopePoint leftPoint, MapEnvelopePoint rightPoint, Rect boundsRect)
        {
            Envelope = envelope;
            LeftPoint = leftPoint;
            RightPoint = rightPoint;
            Bounds = boundsRect;
        }
    }
}
