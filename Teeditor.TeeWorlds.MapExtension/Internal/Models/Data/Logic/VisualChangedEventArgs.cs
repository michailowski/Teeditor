using System;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic
{
    internal class VisualChangedEventArgs : EventArgs
    {
        public object ChangedItem { get; set; }
    }
}
