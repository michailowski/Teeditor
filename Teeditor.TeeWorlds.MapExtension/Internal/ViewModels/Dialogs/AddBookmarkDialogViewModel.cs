using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Models.Tab;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Dialogs
{
    internal class AddBookmarkDialogViewModel : BindableBase
    {
        private SceneCamera _camera;

        public string DefaultLabel => $"Bookmark {_camera?.ViewBookmarks.Count + 1}";

        public void SetTab(ITab tab)
        {
            var sceneManager = tab?.SceneManager as MapSceneManager;

            _camera = sceneManager?.Camera;
        }

        public bool TryAddBookmark(string label)
        {
            label = string.IsNullOrEmpty(label) || string.IsNullOrWhiteSpace(label) ? DefaultLabel : label;

            return _camera?.TryAddCurrentViewBookmark(label) ?? false;
        }
    }
}
