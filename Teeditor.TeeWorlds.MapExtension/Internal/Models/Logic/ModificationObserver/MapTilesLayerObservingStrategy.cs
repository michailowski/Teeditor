using System;
using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Models.ModificationObserving;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.ModificationObserver
{
    internal class MapTilesLayerObservingStrategy : ModificationObservingStrategyBase
    {
        private MapTilesLayer TilesLayer => _observableModel as MapTilesLayer;

        private static string[] _propertiesNames
            = { "Name", "Image", "IsHighDetail", "Width", "Height", "Color", "ColorEnvelope", "ColorEnvelopeOffset" };

        protected override void Initialize()
        {
            TilesLayer.PropertyModificated += TilesLayer_PropertyModificated;
        }

        private void TilesLayer_PropertyModificated(object sender, PropertyModificatedEventArgs e)
        {
            if (Array.IndexOf(_propertiesNames, e.PropertyName) == -1)
                return;

            RaiseModification(e);
        }
    }
}
