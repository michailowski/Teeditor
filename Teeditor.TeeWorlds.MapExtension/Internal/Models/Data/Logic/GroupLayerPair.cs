using Teeditor.Common.Models.Bindable;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic
{
    internal class GroupLayerPair : BindableBase
    {
        private MapLayer _layer;

        public MapGroup Group { get; set; }
        public MapLayer Layer
        {
            get => _layer;
            set => Set(ref _layer, value);
        }
    }
}
