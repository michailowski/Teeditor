using System;
using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Models.ModificationObserving;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data
{
    internal class MapItem : BindableBase, IModificationObservable
    {
        public event EventHandler Modified;

        protected void RaiseModification()
            => Modified?.Invoke(this, EventArgs.Empty);
    }
}
