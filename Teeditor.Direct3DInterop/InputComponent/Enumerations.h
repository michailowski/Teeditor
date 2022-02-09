#include "pch.h"
#pragma once

namespace Teeditor_Direct3DInterop
{
    namespace Enumerations
    {
        using namespace Platform::Metadata;

        public enum class MouseButton : int { None, Left, Middle, Right };
        public enum class MouseInputType : int { None, Move, Pressed, Released, Click, WheelUp, WheelDown, DragStart, Drag, DragEnd };
        [Flags] public enum class MouseKeyModifiers : unsigned int { None = 0, Left = 1, Middle = 2, Right = 4, Shift = 8, Control = 16, Alt = 32 };

        inline MouseKeyModifiers operator | (MouseKeyModifiers lhs, MouseKeyModifiers rhs)
        {
            using T = std::underlying_type_t <MouseKeyModifiers>;
            return static_cast<MouseKeyModifiers>(static_cast<T>(lhs) | static_cast<T>(rhs));
        }

        inline MouseKeyModifiers& operator |= (MouseKeyModifiers& lhs, MouseKeyModifiers rhs)
        {
            lhs = lhs | rhs;
            return lhs;
        }

        inline MouseKeyModifiers operator & (MouseKeyModifiers lhs, MouseKeyModifiers rhs)
        {
            using T = std::underlying_type_t <MouseKeyModifiers>;
            return static_cast<MouseKeyModifiers>(static_cast<T>(lhs) & static_cast<T>(rhs));
        }

        inline MouseKeyModifiers& operator &= (MouseKeyModifiers& lhs, MouseKeyModifiers rhs)
        {
            lhs = lhs & rhs;
            return lhs;
        }

        public enum class KeyboardInputType : int { None, Pressed, Released };
    }
}