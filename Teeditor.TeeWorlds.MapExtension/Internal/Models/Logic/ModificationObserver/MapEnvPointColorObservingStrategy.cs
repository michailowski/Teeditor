using System;
using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Models.ModificationObserving;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.ModificationObserver
{
    internal class MapEnvPointColorObservingStrategy : ModificationObservingStrategyBase
    {
        private MapEnvelopePointColor Point => _observableModel as MapEnvelopePointColor;

        private static string[] _propertiesNames = { "Time", "CurveTypeId", "Color", "Red", "Green", "Blue", "Alpha" };

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
