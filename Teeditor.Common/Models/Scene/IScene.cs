using Microsoft.Graphics.Canvas;
using Teeditor.Common.Models.Components;

namespace Teeditor.Common.Models.Scene
{
    public interface IScene : IKeyboardInteractionItem, IMouseInteractionItem
    {
        bool Activated { get; }

        void SetComponentsManager(IComponentsManager componentsManager);

        void Update();
        void Draw(CanvasDrawingSession drawingSession);
    }
}
