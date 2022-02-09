using Teeditor.Common.Views.Panelbar;

namespace Teeditor.Common.Models.Panelbar
{
    public class PanelItem
    {
        public string Label { get; private set; }
        public PanelControl Panel { get; private set; }

        public PanelItem(string label, PanelControl panel)
        {
            Label = label;
            Panel = panel;
        }
    }
}
