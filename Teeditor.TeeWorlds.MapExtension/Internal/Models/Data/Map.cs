using System;
using Teeditor.Common.Models.IO;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data
{
    internal class Map : EditableEntityBase
    {
        public MapInfo Info { get; private set; }
        public ImagesContainer ImagesContainer { get; private set; }
        public GroupedLayersContainer GroupedLayersContainer { get; private set; }
        public EnvelopesContainer EnvelopesContainer { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public GroupLayerPair CurrentExplorerSelection { get; } = new GroupLayerPair();
        public LayersSelectionsManager SelectionManager { get; } = new LayersSelectionsManager();

        public event EventHandler<VisualChangedEventArgs> VisualChanged;
        public override event EventHandler Loaded;

        public Map(MapInfo info)
        {
            Info = info;
            ImagesContainer = new ImagesContainer();
            GroupedLayersContainer = new GroupedLayersContainer();
            EnvelopesContainer = new EnvelopesContainer();

            GroupedLayersContainer.VisualChanged += GroupedLayersContainer_VisualChanged;
        }

        public void MarkAsLoaded()
        {
            if (IsLoading == false)
                return;

            IsLoading = false;
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        private void GroupedLayersContainer_VisualChanged(object sender, VisualChangedEventArgs e)
        {
            CalculateSize();
            VisualChanged?.Invoke(sender, e);
        }

        public void CalculateSize()
        {
            Width = Height = 0;

            foreach (var group in GroupedLayersContainer.Groups)
            {
                foreach (var layer in group.Layers)
                {
                    if (layer is MapTilesLayer tileLayer)
                    {
                        Width = Math.Max(Width, tileLayer.Width);
                        Height = Math.Max(Height, tileLayer.Height);
                    }
                }
            }
        }
    }
}