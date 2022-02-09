using System.Collections.ObjectModel;
using Teeditor.Common.ViewModels;
using System;
using Teeditor.Common;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Models.Panelbar;

namespace Teeditor.ViewModels
{
    internal class PanelbarViewModel : DynamicViewModel
    {
        private PanelbarManagerBase _panelbarManager;
        private ITab _tab;
        private ObservableCollection<PanelItem> _panels;
        private int _selectedItemIndex = -1;

        public ObservableCollection<PanelItem> Panels
        {
            get => _panels;
            set => Set(ref _panels, value);
        }

        public int SelectedItemIndex
        {
            get => _selectedItemIndex;
            set => Set(ref _selectedItemIndex, value);
        }

        public string State => _tab?.State;

        internal event EventHandler TabUpdated;

        public void SetTab(ITab tab)
        {
            _tab = tab;
            DynamicModel = tab;
            OnPropertyChanged("State");

            if (_panelbarManager == tab?.PanelbarManager)
                return;

            _panelbarManager = tab?.PanelbarManager;

            Panels = _panelbarManager?.Items;

            SelectedItemIndex = _panelbarManager?.SelectedItemIndex ?? -1;

            TabUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void SetSelectedPanel(PanelItem panel)
        {
            if (_panelbarManager != null)
                _panelbarManager.SelectedItemIndex = Panels.IndexOf(panel);

            SelectedItemIndex = Panels.IndexOf(panel);
        }
    }
}
