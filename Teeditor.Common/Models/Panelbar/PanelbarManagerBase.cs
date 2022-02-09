using System.Collections.ObjectModel;
using Teeditor.Common.Models.Tab;

namespace Teeditor.Common.Models.Panelbar
{
    public abstract class PanelbarManagerBase : IPanelbarManager
    {
        public ObservableCollection<PanelItem> Items { get; } = new ObservableCollection<PanelItem>();

        public int SelectedItemIndex { get; set; } = -1;

        public void SetTab(ITab tab)
        {
            foreach (var item in Items)
            {
                item.Panel.ViewModel?.SetTab(tab);
            }
        }
    }
}
