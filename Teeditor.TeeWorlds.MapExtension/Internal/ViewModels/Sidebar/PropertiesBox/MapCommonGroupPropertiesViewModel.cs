using System.ComponentModel;
using System.Numerics;
using Teeditor.Common.ViewModels;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Windows.Foundation;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar.PropertiesBox
{
    internal class MapCommonGroupPropertiesViewModel : DynamicViewModel, IPropertiesBoxItemViewModel
    {
        private MapGroup _model;
        public MapItem Model => _model;

        public string Name
        {
            get => _model.Name;
            set => _model.Name = value;
        }

        public float OffsetX
        {
            get => _model.Offset.X;
            set =>_model.Offset = new Vector2(value, _model.Offset.Y);
        }

        public float OffsetY
        {
            get => _model.Offset.Y;
            set => _model.Offset = new Vector2(_model.Offset.X, value);
        }

        public float ParallaxX
        {
            get => _model.Parallax.X;
            set => _model.Parallax = new Vector2(value, _model.Parallax.Y);
        }

        public float ParallaxY
        {
            get => _model.Parallax.Y;
            set => _model.Parallax = new Vector2(_model.Parallax.X, value);
        }

        public bool UseClipping
        {
            get => _model.UseClipping;
            set => _model.UseClipping = value;
        }

        public double ClipX
        {
            get => _model.Clip.X;
            set => _model.Clip = new Rect(value, _model.Clip.Y, _model.Clip.Width, _model.Clip.Height);
        }

        public double ClipY
        {
            get => _model.Clip.Y;
            set => _model.Clip = new Rect(_model.Clip.X, value, _model.Clip.Width, _model.Clip.Height);
        }

        public double ClipW
        {
            get => _model.Clip.Width;
            set => _model.Clip = new Rect(_model.Clip.X, _model.Clip.Y, value, _model.Clip.Height);
        }

        public double ClipH
        {
            get => _model.Clip.Height;
            set => _model.Clip = new Rect(_model.Clip.X, _model.Clip.Y, _model.Clip.Width, value);
        }

        public MapCommonGroupPropertiesViewModel(MapGroup mapGroup)
        {
            DynamicModel = _model = mapGroup;

            mapGroup.PropertyChanged += MapGroup_PropertyChanged;
        }

        private void MapGroup_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Offset")
            {
                OnPropertyChanged("OffsetX");
                OnPropertyChanged("OffsetY");
            }
            else if (e.PropertyName == "Parallax")
            {
                OnPropertyChanged("ParallaxX");
                OnPropertyChanged("ParallaxY");
            }
            else if (e.PropertyName == "Clip")
            {
                OnPropertyChanged("ClipX");
                OnPropertyChanged("ClipY");
                OnPropertyChanged("ClipW");
                OnPropertyChanged("ClipH");
            }
        }
    }
}
