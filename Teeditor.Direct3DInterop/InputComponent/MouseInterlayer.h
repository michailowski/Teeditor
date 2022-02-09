#include "InputComponent/Enumerations.h"
#include "InputComponent/KeyboardInterlayer.h"
#include "InputComponent/MouseInputQueue.h"
#include "InputComponent/MouseState.h"
#pragma once

using namespace Windows::Foundation::Numerics;
using namespace Windows::Foundation;
using namespace Windows::Graphics::Display;
using namespace Teeditor_Direct3DInterop::Enumerations;
using namespace Windows::UI::Xaml::Input;
using ButtonState = Mouse::ButtonStateTracker::ButtonState;

namespace Teeditor_Direct3DInterop
{
    [Windows::Foundation::Metadata::WebHostHidden]
    public ref class MouseInterlayer sealed
    {
    public:
        MouseInterlayer(Window^ window, UIElement^ relativeTo, KeyboardInterlayer^ keyboard);

        property MouseInputQueue^ Queue;
        property MouseState^ State;

    internal:
        void Update();

    private:
        std::unique_ptr<Mouse> mouse;
        Mouse::ButtonStateTracker tracker;
        
        void OnDpiChanged(DisplayInformation^ sender, Object^ args);
        void RelativeContainerPointerEntered(Object^ sender, PointerRoutedEventArgs^ args);
        void RelativeContainerPointerExited(Object^ sender, PointerRoutedEventArgs^ args);

        void UpdateMouseState();
        void UpdateKeyModifiers();
        void UpdateMouseInputs();

        void ProcessRelativeMousePosition();
        void ProcessKeyModifier(bool condition, MouseKeyModifiers keyModifier);
        void ProcessMouseMove();
        void ProcessMouseButton(MouseButton button, MouseButtonState buttonState);
        void ProcessMouseWheel();

        void EnqueueMouseInput(MouseButton button, MouseInputType inputType);

        Agile<Window^> currentWindow;
        UIElement^ relativeContainer;
        KeyboardInterlayer^ keyboard;

        MouseKeyModifiers currentKeyModifiers;

        float2 absolutePosition;
        float2 relativePosition;
        float2 prevRelativePosition;
        bool isRelativeContainerPointed;
    };
}