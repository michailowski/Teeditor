using Microsoft.Graphics.Canvas;
using Teeditor_Direct3DInterop;
using Teeditor_TeeWorlds_Direct3DInterop;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.Common.Models.Commands;
using Teeditor.Common.Models.Scene;
using Teeditor.Common.Models.Components;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager.Scenes
{
    internal abstract class LayerOverlayScene : IScene
    {
        protected GraphicsComponent _graphicsComponent;

        protected MapGroup _group;
        protected MapImage _image;
        protected SceneCamera _camera;

        private bool _preventLayerChanges = false;

        public abstract bool Activated { get; }
        public bool PreventLayerChanges { get => _preventLayerChanges; set => _preventLayerChanges = value; }

        public void SetComponentsManager(IComponentsManager componentsManager)
        {
            var mapComponentsManager = componentsManager as ComponentsManager;

            _graphicsComponent = mapComponentsManager.GraphicsComponent;
        }

        public abstract void SetDependencies(ExplorerSelectionSnapshot selectionSnapshot, SceneCamera camera, Map map);

        public abstract void Draw(CanvasDrawingSession drawingSession);

        public abstract void ProcessKeyboardInput(KeyboardInput input, out bool handled);

        public abstract void ProcessMouseInput(MouseInput input, out bool handled);

        public virtual void Update()
        {
            // Do nothing
        }

        protected void ExecuteCommand(IUndoRedoableCommand command)
        {
            CommandExecuted?.Invoke(this, command);
        }

        public event CommandExecuteHandler CommandExecuted;
    }
}
