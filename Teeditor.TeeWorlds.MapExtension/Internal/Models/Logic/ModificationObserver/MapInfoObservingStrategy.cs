using System;
using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Models.ModificationObserving;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.ModificationObserver
{
    internal class MapInfoObservingStrategy : ModificationObservingStrategyBase
    {
        private MapInfo Info => _observableModel as MapInfo;

        private static string[] _propertiesNames = { "Author", "MapVersion", "Credits", "License" };

        protected override void Initialize()
        {
            Info.PropertyModificated += Info_PropertyModificated;
        }

        private void Info_PropertyModificated(object sender, PropertyModificatedEventArgs e)
        {
            if (Array.IndexOf(_propertiesNames, e.PropertyName) == -1)
                return;

            RaiseModification(e);
        }
    }
}
