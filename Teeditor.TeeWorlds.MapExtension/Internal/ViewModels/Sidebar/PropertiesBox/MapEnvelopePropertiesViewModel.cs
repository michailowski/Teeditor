using Teeditor.Common.ViewModels;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar.PropertiesBox
{
    internal class MapEnvelopePropertiesViewModel : DynamicViewModel, IPropertiesBoxItemViewModel
    {
        private MapEnvelope _model;
        public MapItem Model => _model;

        public string Name
        {
            get => _model.Name;
            set => _model.Name = value;
        }

        public bool IsSynchronized
        {
            get => _model.IsSynchronized;
            set => _model.IsSynchronized = value;
        }

        public MapEnvelopePropertiesViewModel(MapEnvelope mapEnvelope)
        {
            DynamicModel = _model = mapEnvelope;
        }
    }
}
