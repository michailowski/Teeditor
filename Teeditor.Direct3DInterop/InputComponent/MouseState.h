#include "pch.h"
#pragma once

namespace Teeditor_Direct3DInterop
{
    public value struct MouseButtonState
    {
    public:
        bool IsPressed;
        bool WasPressed;
        bool WasReleased;
        bool WasUP;
        bool WasHeld;
    };

    public ref class MouseState sealed
    {
    public:
        property float2 Position 
        {
            float2 get() { return position; }
        }

        property MouseButtonState LeftButton 
        {
            MouseButtonState get() { return leftButton; }
        }

        property MouseButtonState MiddleButton 
        {
            MouseButtonState get() { return middleButton; }
        }

        property MouseButtonState RightButton 
        {
            MouseButtonState get() { return rightButton; }
        }

        property int WheelDelta 
        {
            int get() { return wheelDelta; }
        }

    internal:
        float2 position;
        MouseButtonState leftButton;
        MouseButtonState middleButton;
        MouseButtonState rightButton;
        int wheelDelta;
    };

}