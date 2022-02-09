using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager.Drawers;
using System.Collections.ObjectModel;
using Teeditor.Common.Models.Commands;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data
{
    internal class MapQuadsLayer : MapLayer
    {
        private string _name;
        private MapImage _image;
        private ObservableCollection<MapQuad> _quads;

        private static QuadsLayerDrawStrategy _drawStrategy;

        [ModificationCommandLabel("Quads layer name changed")]
        public override string Name
        {
            get => string.IsNullOrEmpty(_name) ? "Quads" : _name;
            set => Set(ref _name, value, nameof(_name));
        }

        public int ImageId { get; set; } = -1;

        [ModificationCommandLabel("Quads layer image changed")]
        public MapImage Image
        {
            get => _image;
            set => SetImage(value);
        }

        public ObservableCollection<MapQuad> Quads => _quads;

        public MapQuadsLayer(ObservableCollection<MapQuad> quads)
        {
            _quads = quads;
        }

        public MapQuadsLayer()
        {
            _quads = new ObservableCollection<MapQuad>();
        }

        private void SetImage(MapImage image)
        {
            _image?.RemoveCarrier(this);

            Set(ref _image, image, nameof(_image), nameof(Image));

            _image?.AddCarrier(this);
        }

        public void ResetImage() => Image = null;

        protected override ILayerDrawStrategy GetDrawStrategy()
        {
            if (_drawStrategy == null)
                _drawStrategy = new QuadsLayerDrawStrategy();

            _drawStrategy.SetLayer(this);

            return _drawStrategy;
        }
    }
}
