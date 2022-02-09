using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teeditor.Common.ViewModels;
using Teeditor.TeeWorlds.MapExtension.Internal.Enumerations;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar.PropertiesBox
{
    internal class MapTilesLayerPropertiesViewModel : DynamicViewModel, IPropertiesBoxItemViewModel
    {
        private MapTilesLayer _model;
        public MapItem Model => _model;

        public ReadOnlyObservableCollection<MapImage> Images { get; }
        public ReadOnlyObservableCollection<MapEnvelope> Envelopes { get; }

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

        public Color Color
        {
            get => _model.Color;
            set => _model.SetColorWithExpectation(value);
        }

        public MapEnvelope ColorEnvelope
        {
            get => _model.ColorEnvelope;
            set => _model.ColorEnvelope = value;
        }

        public int ColorEnvelopeOffset
        {
            get => _model.ColorEnvelopeOffset;
            set => _model.ColorEnvelopeOffset = value;
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

        public MapTilesLayerPropertiesViewModel(MapTilesLayer mapLayer, ImagesContainer imagesContainer, EnvelopesContainer envelopesContainer)
        {
            Images = imagesContainer.Items;
            Envelopes = envelopesContainer.Items;

            DynamicModel = _model = mapLayer;
        }

        public void ResetColor() => _model.SetColorWithExpectation(Colors.White);

        public void ResetImage() => _model.ResetImage();

        public void ResetColorEnvelope() => _model.ResetColorEnvelope();

        public void ShiftLeft() => _model.ShiftLeft();

        public void ShiftRight() => _model.ShiftRight();

        public void ShiftTop() => _model.ShiftTop();

        public void ShiftBottom() => _model.ShiftBottom();

        public IEnumerable<MapEnvelope> GetOnlyColorEnvelopes(ReadOnlyObservableCollection<MapEnvelope> envelopes)
            => envelopes.Where(x => x.Type == EnvelopeType.Color);
    }
}
