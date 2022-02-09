using System.Runtime.CompilerServices;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Utilities;
using Teeditor.Common.ViewModels;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Toolbar
{
    internal class AnimationToolViewModel : ToolViewModelBase
    {
        private SceneTimer _sceneTimer;

        public bool IsStarted
        {
            get => _sceneTimer.IsStarted;
            set => _sceneTimer.IsStarted = value;
        }

        public AnimationToolViewModel()
        {
            Label = "Animation Tool";
            MenuText = "Animation Tool";
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["AnimationToolIconPath"]) };
        }

        public override void SetTab(ITab tab)
        {
            var sceneManager = tab?.SceneManager as MapSceneManager;
            _sceneTimer = sceneManager?.Timer;

            DynamicModel = _sceneTimer;

            OnPropertyChanged("IsStarted");
        }

        public void ResetTimer() => _sceneTimer?.Reset();
    }
}
