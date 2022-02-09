using System.Collections.ObjectModel;
using Teeditor.Common.ViewModels;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar.PropertiesBox
{
    internal class MapQuadsLayerPropertiesViewModel : DynamicViewModel, IPropertiesBoxItemViewModel
    {
        private MapQuadsLayer _model;
        public MapItem Model => _model;

        public ReadOnlyObservableCollection<MapImage> Images { get; }

        public string Name
        {
            get => _model.Name;
            set => _model.Name = value;
        }

        public MapImage Image
        {
            get => _model.Image;
            set => _model.Image = value;
        }

        public bool IsHighDetail
        {
            get => _model.IsHighDetail;
            set => _model.IsHighDetail = value;
        }

        public MapQuadsLayerPropertiesViewModel(MapQuadsLayer mapLayer, ImagesContainer imagesContainer)
        {
            Images = imagesContainer.Items;

            DynamicModel = _model = mapLayer;
        }

        public void ResetImage() => _model.ResetImage();
    }
}
