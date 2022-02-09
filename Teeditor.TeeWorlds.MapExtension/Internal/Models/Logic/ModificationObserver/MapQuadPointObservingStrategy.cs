using System;
using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Models.ModificationObserving;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.ModificationObserver
{
    internal class MapQuadPointObservingStrategy : ModificationObservingStrategyBase
    {
        private MapQuadPoint Point => _observableModel as MapQuadPoint;

        private static string[] _propertiesNames
            = { "PositionX", "PositionY", "Color", "TextureX", "TextureY" };

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
