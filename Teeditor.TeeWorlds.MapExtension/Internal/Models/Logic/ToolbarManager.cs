using Teeditor.TeeWorlds.MapExtension.Internal.Views.Toolbar;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Toolbar;
using Teeditor.Common.Models.Toolbar;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic
{
    internal class ToolbarManager : ToolbarManagerBase
    {
        public ToolbarManager()
        {
            var viewToolViewModel = new ViewToolViewModel();
            Items.Add(new ViewToolControl(viewToolViewModel));

            var cameraToolViewModel = new CameraToolViewModel();
            Items.Add(new CameraToolControl(cameraToolViewModel));

            var layerSelectionToolViewModel = new LayerSelectionToolViewModel();
            Items.Add(new LayerSelectionToolControl(layerSelectionToolViewModel));

            var quadsLayerToolViewModel = new QuadsLayerToolViewModel();
            Items.Add(new QuadsLayerToolControl(quadsLayerToolViewModel));

            var animationToolViewModel = new AnimationToolViewModel();
            Items.Add(new AnimationToolControl(animationToolViewModel));
        }
    }
}
