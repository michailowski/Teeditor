using System.Collections.ObjectModel;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Views.Toolbar;

namespace Teeditor.Common.Models.Toolbar
{
    internal interface IToolbarManager
    {
        ObservableCollection<ToolControl> Items { get; }

        void SetTab(ITab tab);
    }
}
