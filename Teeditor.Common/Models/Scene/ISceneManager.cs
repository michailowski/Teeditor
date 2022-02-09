using Microsoft.Graphics.Canvas;
using Teeditor_Direct3DInterop;
using System.Numerics;
using Teeditor.Common.Models.Commands;
using Teeditor.Common.Models.Tab;

namespace Teeditor.Common.Models.Scene
{
    public delegate void CommandExecuteHandler(object sender, IUndoRedoableCommand command);

    public interface ISceneManager
    {
        bool IsIdle { get; }

        void SetSize(Vector2 size);

        void ProcessMouseInput(MouseInput input);
        void ProcessKeyboardInput(KeyboardInput input);

        void SetTab(ITab tab);

        void Update();
        void Draw(CanvasDrawingSession drawingSession);

        event CommandExecuteHandler CommandExecuted;
    }
}
