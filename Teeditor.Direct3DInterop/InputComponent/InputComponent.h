#pragma once

namespace Teeditor_Direct3DInterop
{
    [Windows::Foundation::Metadata::WebHostHidden]
    public ref class InputComponent sealed
    {
    public:
        InputComponent(Window^ window, UIElement^ relativeTo);

        property KeyboardInterlayer^ Keyboard;
        property MouseInterlayer^ Mouse;

        void Update();
    };

}