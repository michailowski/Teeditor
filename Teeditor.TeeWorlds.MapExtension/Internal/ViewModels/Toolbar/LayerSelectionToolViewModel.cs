using Teeditor.Common.Models.Tab;
using Teeditor.Common.Utilities;
using Teeditor.Common.ViewModels;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Toolbar
{
    internal class LayerSelectionToolViewModel : ToolViewModelBase
    {
        private GroupLayerPair _currentExplorerSelection;
        private LayersSelectionsManager _selectionsManager;
        private ILayerSelection _selection;

        public bool IsTransformationAllowed => _selection?.IsTransformationAllowed ?? false;

        public LayerSelectionToolViewModel()
        {
            Label = "Layer Selection Tool";
            MenuText = "Layer Selection Tool";
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["LayerSelectionToolIconPath"]) };
        }

        public override void SetTab(ITab tab)
        {
            if (_currentExplorerSelection != null)
            {
                _currentExplorerSelection.PropertyChanged -= CurrentExplorerSelection_PropertyChanged;
            }

            var map = tab?.Data as Map;

            _currentExplorerSelection = map?.CurrentExplorerSelection;
            _selectionsManager = map?.SelectionManager;

            RefreshSelection();

            _currentExplorerSelection.PropertyChanged += CurrentExplorerSelection_PropertyChanged;
        }

        private void RefreshSelection()
        {
            if (_currentExplorerSelection.Layer is MapTilesLayer)
            {
                _selection = _selectionsManager?.TilesLayerSelection;
                DynamicModel = _selection;
            }
            else if (_currentExplorerSelection.Layer is MapQuadsLayer)
            {
                _selection = _selectionsManager?.QuadsLayerSelection;
                DynamicModel = _selection;
            }

            OnPropertyChanged("IsTransformationAllowed");
        }

        private void CurrentExplorerSelection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Layer")
            {
                RefreshSelection();
            }
        }

        public void FlipVertical() => _selection?.FlipVertical();
        public void FlipHorizontal() => _selection?.FlipHorizontal();
        public void RotateClockwise() => _selection?.Rotate(90);
        public void RotateCounterClockwise() => _selection?.Rotate(-90);
    }
}
