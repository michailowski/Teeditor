using System.Collections.Generic;
using System.Numerics;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Utilities;
using Teeditor.Common.ViewModels;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Dialogs;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Toolbar
{
    internal class CameraToolViewModel : ToolViewModelBase
    {
        private SceneCamera _camera;
        private AddBookmarkDialogViewModel _addBookmarkDialogViewModel;

        public AddBookmarkDialogViewModel AddBookmarkDialogViewModel => _addBookmarkDialogViewModel;
        public Dictionary<Vector3, string> Bookmarks => _camera?.ViewBookmarks;

        public CameraToolViewModel()
        {
            Label = "Camera Tool";
            MenuText = "Camera Tool";
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["CameraToolIconPath"]) };

            _addBookmarkDialogViewModel = new AddBookmarkDialogViewModel();
        }

        public override void SetTab(ITab tab)
        {
            var sceneManager = tab?.SceneManager as MapSceneManager;

            _camera = sceneManager?.Camera;

            _addBookmarkDialogViewModel.SetTab(tab);
        }

        public void ResetPosition() => _camera?.ResetPosition();

        public void ResetZoom() => _camera?.ResetZoom();

        public void ZoomIn() => _camera?.TryIncreaseZoomLevel();

        public void ZoomOut() => _camera?.TryDecreaseZoomLevel();

        public void GoTo(KeyValuePair<Vector3, string> bookmark) => _camera?.GoToView(bookmark.Key);
    }
}
