using System;
using Teeditor.Common.Models.Tab;

namespace Teeditor.Models
{
    internal class TabBuildingEndedEventArgs : EventArgs
    {
        public TabBase CreatedTab { get; }
        public bool IsQueueEmpty { get; }
        public TabBuildingEndedEventArgs(TabBase createdTab, bool isQueueEmpty)
        {
            CreatedTab = createdTab;
            IsQueueEmpty = isQueueEmpty;
        }
    }
}
