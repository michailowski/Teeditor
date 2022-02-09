using Teeditor.Common.Models.Tab;
using Teeditor.Common.Utilities;
using Teeditor.Common.ViewModels;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Toolbar
{
    internal class QuadsLayerToolViewModel : ToolViewModelBase
    {
        private GroupLayerPair _currentExplorerSelection;
        private bool _isAddingAllowed;

        public bool IsAddingAllowed 
        { 
            get => _isAddingAllowed;
            set => Set(ref _isAddingAllowed, value);
        }

        public QuadsLayerToolViewModel()
        {
            Label = "Quads Layer Tool";
            MenuText = "Quads Layer Tool";
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["QuadsLayerToolIconPath"]) };
        }

        public override void SetTab(ITab tab)
        {
            if (_currentExplorerSelection != null)
            {
                _currentExplorerSelection.PropertyChanged -= CurrentExplorerSelection_PropertyChanged;
            }

            var map = tab?.Data as Map;

            _currentExplorerSelection = map?.CurrentExplorerSelection;

            IsAddingAllowed = _currentExplorerSelection?.Layer is MapQuadsLayer;
            _currentExplorerSelection.PropertyChanged += CurrentExplorerSelection_PropertyChanged;
        }

        private void CurrentExplorerSelection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Layer")
            {
                IsAddingAllowed = _currentExplorerSelection.Layer is MapQuadsLayer;
            }
        }

        public void AddQuad()
        {
            var quadsLayer = _currentExplorerSelection.Layer as MapQuadsLayer;

            var quad = new MapQuad();
            quadsLayer.Quads.Add(quad);
        }
    }
}
