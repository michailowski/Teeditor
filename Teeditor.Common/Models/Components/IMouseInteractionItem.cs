using Teeditor_Direct3DInterop;

namespace Teeditor.Common.Models.Components
{
    public interface IMouseInteractionItem
    {
        void ProcessMouseInput(MouseInput input, out bool handled);
    }
}
