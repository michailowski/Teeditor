using System;
using Teeditor.Common.Views.Toolbar;

namespace Teeditor.Common.Models.Toolbar
{
    public class ToolbarItemChangedEventArgs : EventArgs
    {
        public ToolControl Tool { get; }

        public ToolbarItemChangedEventArgs(ToolControl tool)
        {
            Tool = tool;
        }
    }
}
