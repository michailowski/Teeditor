using System;
using Windows.UI.Xaml.Controls;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager.Drawers;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;
using Teeditor.Common.Models.Commands;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data
{
    internal abstract class MapLayer : MapItem
    {
        private bool _isSelected = false;
        private bool _isVisible = true;
        private bool _isSaved = true;
        private bool _isHighDetail = false;

        private Grid _uiElement;

        public abstract string Name { get; set; }

        [ModificationCommandLabel("Layer high detail changed")]
        public bool IsHighDetail
        {
            get => _isHighDetail;
            set => Set(ref _isHighDetail, value, nameof(_isHighDetail));
        }

        public Guid Guid { get; } = Guid.NewGuid();

        public Grid UIElement
        {
            get => _uiElement;
            set => Set(ref _uiElement, value);
        }

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

        public ILayerDrawStrategy DrawStrategy => GetDrawStrategy();

        public event EventHandler<VisualChangedEventArgs> VisualChanged;

        protected abstract ILayerDrawStrategy GetDrawStrategy();

        protected void RaiseVisualChanges()
        {
            VisualChanged?.Invoke(this, new VisualChangedEventArgs() { ChangedItem = this });
        }
    }
}
