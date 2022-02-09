using System;
using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Models.ModificationObserving;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.ModificationObserver
{

    internal class MapQuadObservingStrategy : ModificationObservingStrategyBase
    {
        private MapQuad Quad => _observableModel as MapQuad;

        private static string[] _propertiesNames
            = { "PosEnvIndex", "PosEnvOffset", "ColorEnvIndex", "ColorEnvOffset" };

        protected override void Initialize()
        {
            Quad.PropertyModificated += Quad_PropertyModificated;

            foreach (var point in Quad.Points)
            {
                Add(point, new MapQuadPointObservingStrategy());
            }
        }

        private void Quad_PropertyModificated(object sender, PropertyModificatedEventArgs e)
        {
            if (Array.IndexOf(_propertiesNames, e.PropertyName) == -1)
                return;

            RaiseModification(e);
        }
    }
}
