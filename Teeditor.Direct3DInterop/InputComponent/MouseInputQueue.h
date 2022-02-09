#include "pch.h"
#include "InputComponent/Enumerations.h"
#pragma once

using namespace Windows::Foundation::Numerics;
using namespace Windows::Foundation::Collections;
using namespace Platform::Collections;
using namespace Teeditor_Direct3DInterop::Enumerations;

namespace Teeditor_Direct3DInterop
{
    public value struct MouseInput
    {
    public:
        float2 Position;
        //float2 DragDeltaPosition;
        MouseButton Button;
        MouseInputType Type;
        MouseKeyModifiers KeyModifiers;
    };

    struct MouseInputEqual /*: public std::binary_function<const MouseInput, const MouseInput, bool>*/
    {
        bool operator()(const MouseInput& _Left, const MouseInput& _Right) const
        {
            return _Left.Type == _Right.Type
                && _Left.Button == _Right.Button
                && _Left.Position == _Right.Position;
        };
    };

    public ref class MouseInputQueue sealed
    {
    public:
        MouseInputQueue() {
            mouseQueue = ref new Vector<MouseInput, MouseInputEqual>();
        }

        MouseInput Dequeue() {
            if (mouseQueue->Size == 0)
                return MouseInput();

            auto input = mouseQueue->GetAt(0);
            mouseQueue->RemoveAt(0);

            return input;
        }

        property int Count {
            int get() {
                return mouseQueue->Size;
            }
        }

    internal:
        void Enqueue(MouseInput input) {
            mouseQueue->Append(input);
        }

        void Clear() {
            mouseQueue->Clear();
        }

    private:
        IVector<MouseInput>^ mouseQueue;
    };
}
