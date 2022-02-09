using System;
using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Models.ModificationObserving;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.ModificationObserver
{
    internal class MapEnvPointPositionObservingStrategy : ModificationObservingStrategyBase
    {
        private MapEnvelopePointPosition Point => _observableModel as MapEnvelopePointPosition;

        private static string[] _propertiesNames = { "Time", "CurveTypeId", "X", "Y", "Rotate" };

        protected override void Initialize()
        {
            Point.PropertyModificated += Point_PropertyModificated;
        }

        private void Point_PropertyModificated(object sender, PropertyModificatedEventArgs e)
        {
            if (Array.IndexOf(_propertiesNames, e.PropertyName) == -1)
                return;

            RaiseModification(e);
        }
    }
}
