#include <InputComponent/KeyboardState.h>
#include <InputComponent/Enumerations.h>
#pragma once

using namespace ::DirectX;
using namespace Windows::UI::Core;
using namespace Windows::UI::Xaml;
using namespace Windows::Foundation::Collections;
using namespace Platform;
using namespace Platform::Collections;
using namespace Teeditor_Direct3DInterop::Enumerations;

namespace Teeditor_Direct3DInterop
{
    public value struct KeyboardInput
    {
    public:
        Keys Key;
        KeyboardInputType Type;
    };

    struct KeyboardInputEqual /*: public std::binary_function<const KeyboardInput, const KeyboardInput, bool>*/
    {
        bool operator()(const KeyboardInput& _Left, const KeyboardInput& _Right) const
        {
            return _Left.Key == _Right.Key
                && _Left.Type == _Right.Type;
        };
    };

    public ref class KeyboardInputQueue sealed
    {
    public:
        KeyboardInputQueue() {
            keyboardQueue = ref new Vector<KeyboardInput, KeyboardInputEqual>();
        }

        KeyboardInput Dequeue() {
            if (keyboardQueue->Size == 0)
                return KeyboardInput();

            auto input = keyboardQueue->GetAt(0);
            keyboardQueue->RemoveAt(0);

            return input;
        }

        property int Count {
            int get() {
                return keyboardQueue->Size;
            }
        }

    internal:
        void Enqueue(KeyboardInput input) {
            keyboardQueue->Append(input);
        }
        void Clear() {
            keyboardQueue->Clear();
        }

    private:
        IVector<KeyboardInput>^ keyboardQueue;
    };

    [Windows::Foundation::Metadata::WebHostHidden]
    public ref class KeyboardInterlayer sealed
    {
    public:
        KeyboardInterlayer(Window^ window);

        property KeyboardInputQueue^ Queue;
        property KeyboardState^ State;

        bool IsKeyDown(Keys key);
        bool IsKeyUp(Keys key);

    internal:
        void Update();

    private:
        std::unique_ptr<DirectX::Keyboard> keyboard;
        DirectX::Keyboard::KeyboardStateTracker tracker;
    };
}