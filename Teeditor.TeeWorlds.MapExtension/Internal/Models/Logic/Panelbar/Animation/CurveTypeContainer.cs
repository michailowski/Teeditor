using Windows.Foundation;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.Panelbar.Animation
{
    internal class CurveTypeContainer
    {
        public string Name { get; }
        public int CurveTypeId { get; }
        public bool IsStandard { get; }
        public bool IsEditable { get; }
        public string PathData { get; }
        public Point? StartPoint { get; }
        public Point? EndPoint { get; }

        public CurveTypeContainer(string name, int curveTypeId, bool isStandard, bool isEditable, string pathData = null, Point? startPoint = null, Point? endPoint = null)
        {
            Name = name;
            CurveTypeId = curveTypeId;
            IsStandard = isStandard;
            IsEditable = isEditable;
            PathData = pathData;
            StartPoint = startPoint;
            EndPoint = endPoint;
        }
    }
}
