using System;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Windows.Foundation;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.Panelbar.Animation
{
    internal struct Keyframe
    {
        public MapEnvelope Envelope { get; }
        public MapEnvelopePoint Point { get; }
        public bool IsEmpty => Point == null;

        public Rect Bounds { get; }

        public TimeSpan MinTime { get; }
        public TimeSpan MaxTime { get; }

        public static Keyframe Empty => new Keyframe(null, null, Rect.Empty);

        public Keyframe(MapEnvelope envelope, MapEnvelopePoint point, Rect boundsRect)
        {
            Envelope = envelope;
            Point = point;
            Bounds = boundsRect;

            if (envelope == null || point == null)
                return;

            var pointIndex = envelope.Points.IndexOf(point);

            MinTime = pointIndex > 0 ? envelope.Points[pointIndex - 1].Time : TimeSpan.Zero;
            MaxTime = pointIndex < envelope.Points.Count - 1 ? envelope.Points[pointIndex + 1].Time : TimeSpan.MaxValue;
        }
    }
}
