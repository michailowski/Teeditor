using Teeditor_Direct3DInterop;

namespace Teeditor.Common.Models.Components
{
    public interface IKeyboardInteractionItem
    {
        void ProcessKeyboardInput(KeyboardInput input, out bool handled);
    }
}
