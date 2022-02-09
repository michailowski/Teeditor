using Teeditor.Common.Models.Panelbar;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Panelbar;
using Teeditor.TeeWorlds.MapExtension.Internal.Views.Panelbar.Animation;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic
{
    internal class PanelbarManager : PanelbarManagerBase
    {
        public PanelbarManager()
        {
            var animationPanelViewModel = new AnimationPanelViewModel();
            var animationPanel = new AnimationPanelControl(animationPanelViewModel);
            Items.Add(new PanelItem("Animation", animationPanel));
        }
    }
}
