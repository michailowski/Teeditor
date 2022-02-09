using System.Collections.ObjectModel;
using Teeditor.Common.Models.Tab;

namespace Teeditor.Common.Models.Panelbar
{
    internal interface IPanelbarManager
    {
        ObservableCollection<PanelItem> Items { get; }

        void SetTab(ITab tab);
    }
}
