using Teeditor.Common.ViewModels;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar.PropertiesBox
{
    internal class MapGameLayerPropertiesViewModel : DynamicViewModel, IPropertiesBoxItemViewModel
    {
        private MapTilesLayer _model;
        public MapItem Model => _model;

        public string Name
        {
            get => _model.Name;
            set => _model.Name = value;
        }

        public int Width
        {
            get => _model.Width;
            set => _model.Width = value;
        }

        public int Height
        {
            get => _model.Height;
            set => _model.Height = value;
        }

        public MapGameLayerPropertiesViewModel(MapTilesLayer mapLayer)
        {
            DynamicModel = _model = mapLayer;
        }

        public void ShiftLeft() => _model.ShiftLeft();

        public void ShiftRight() => _model.ShiftRight();

        public void ShiftTop() => _model.ShiftTop();

        public void ShiftBottom() => _model.ShiftBottom();
    }
}
