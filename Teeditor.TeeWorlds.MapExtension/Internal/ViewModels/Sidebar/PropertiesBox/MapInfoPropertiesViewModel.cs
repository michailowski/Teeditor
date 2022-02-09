using Teeditor.Common.ViewModels;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar.PropertiesBox
{
    internal class MapInfoPropertiesViewModel : DynamicViewModel, IPropertiesBoxItemViewModel
    {
        private MapInfo _model;
        public MapItem Model => _model;

        public string Author
        {
            get => _model.Author;
            set => _model.Author = value;
        }

        public string Credits
        {
            get => _model.Credits;
            set => _model.Credits = value;
        }

        public string License
        {
            get => _model.License;
            set => _model.License = value;
        }

        public string MapVersion
        {
            get => _model.MapVersion;
            set => _model.MapVersion = value;
        }

        public MapInfoPropertiesViewModel(MapInfo mapInfo)
        {
            DynamicModel = _model = mapInfo;
        }
    }
}
