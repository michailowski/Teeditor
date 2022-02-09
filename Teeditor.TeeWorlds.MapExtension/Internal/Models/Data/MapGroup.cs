using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Teeditor.Common.Models.Commands;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data
{
    internal class MapGroup : MapItem
    {
        private bool _isGameGroup = false;
        private bool _isSelected = false;
        private bool _isVisible = true;
        private bool _isSaved = true;
        private bool _isExpanded = false;
        private Grid _uiElement;

        private string _name;
        private Vector2 _offset;
        private Vector2 _parallax = new Vector2(100);
        private bool _useClipping;
        private Rect _clip;
        private bool _hasLayers;
        private ObservableCollection<MapLayer> _layers;

        [ModificationCommandLabel("Group name changed")]
        public string Name
        {
            get => GetName();
            set => Set(ref _name, value, nameof(_name));
        }

        [ModificationCommandLabel("Group offset changed")]
        public Vector2 Offset
        {
            get => _offset;
            set => Set(ref _offset, value, nameof(_offset));
        }

        [ModificationCommandLabel("Group parallax changed")]
        public Vector2 Parallax
        {
            get => _parallax;
            set => Set(ref _parallax, value, nameof(_parallax));
        }

        [ModificationCommandLabel("Group use of clipping changed")]
        public bool UseClipping
        {
            get => _useClipping;
            set => Set(ref _useClipping, value, nameof(_useClipping));
        }

        [ModificationCommandLabel("Group clipping boundaries changed")]
        public Rect Clip
        {
            get => _clip;
            set => Set(ref _clip, value, nameof(_clip));
        }

        public ReadOnlyObservableCollection<MapLayer> Layers { get; }

        public Guid Guid { get; } = Guid.NewGuid();

        public Grid UIElement
        {
            get => _uiElement;
            set => Set(ref _uiElement, value);
        }

        public bool HasLayers
        {
            get => _hasLayers;
            private set => Set(ref _hasLayers, value);
        }

        public bool IsGameGroup { get => _isGameGroup; set => _isGameGroup = value; }

        public bool IsSelected
        {
            get => _isSelected;
            set => Set(ref _isSelected, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => Set(ref _isVisible, value);
        }

        public bool IsSaved
        {
            get => _isSaved;
            set => Set(ref _isSaved, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => Set(ref _isExpanded, value);
        }

        public Matrix3x2 TransformMatrix3X2 { get; set; } = Matrix3x2.Identity;

        public Matrix4x4 TransformMatrix4X4 { get; set; } = Matrix4x4.Identity;

        public event EventHandler<VisualChangedEventArgs> VisualChanged;

        public MapGroup()
        {
            _layers = new ObservableCollection<MapLayer>();
            Layers = new ReadOnlyObservableCollection<MapLayer>(_layers);
        }

        private string GetName()
        {
            if (IsGameGroup)
                return "Game";

            return string.IsNullOrEmpty(_name) ? "Group" : _name;
        }

        public void Add(MapLayer layer)
        {
            _layers.Add(layer);
            HasLayers = Layers.Count > 0;

            layer.VisualChanged += Layer_VisualChanged;
            VisualChanged?.Invoke(this, new VisualChangedEventArgs() { ChangedItem = this });
        }

        public void Insert(int index, MapLayer layer)
        {
            _layers.Insert(index, layer);
            HasLayers = Layers.Count > 0;

            layer.VisualChanged += Layer_VisualChanged;
            VisualChanged?.Invoke(this, new VisualChangedEventArgs() { ChangedItem = this });
        }

        public void Remove(MapLayer layer)
        {
            _layers.Remove(layer);
            HasLayers = Layers.Count > 0;

            if (layer is MapTilesLayer tilesLayer)
            {
                tilesLayer.Image = null;
            }
            else if (layer is MapQuadsLayer quadsLayer)
            {
                quadsLayer.Image = null;
            }    

            layer.VisualChanged -= Layer_VisualChanged;
            VisualChanged?.Invoke(this, new VisualChangedEventArgs() { ChangedItem = this });
        }

        public void UnselectLayers()
        {
            foreach (var layer in Layers)
            {
                if (layer.IsSelected)
                {
                    layer.IsSelected = false;
                }
            }
        }

        private void Layer_VisualChanged(object sender, VisualChangedEventArgs e)
            => VisualChanged?.Invoke(sender, e);
    }
}
