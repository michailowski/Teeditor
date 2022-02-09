#include "pch.h"
#include "InputComponent/KeyboardInterlayer.h"

using namespace Teeditor_Direct3DInterop;

KeyboardInterlayer::KeyboardInterlayer(Window^ window)
{
    keyboard = std::make_unique<Keyboard>();
    keyboard->SetWindow(window->CoreWindow);

    Queue = ref new KeyboardInputQueue();
    State = ref new KeyboardState();
}

void KeyboardInterlayer::Update()
{
    Queue->Clear();

    auto rawState = keyboard->GetState();
    tracker.Update(rawState);

    for (const auto key : UsedKeys)
    {
        Keyboard::Keys keyboardKey = static_cast<Keyboard::Keys>(key);

        if (tracker.IsKeyPressed(keyboardKey))
        {
            KeyboardInput input = {
                key,
                KeyboardInputType::Pressed
            };

            unsigned int index;

            if (!State->pressedKeys->IndexOf(key, &index))
            {
                State->pressedKeys->Append(key);
            }

            Queue->Enqueue(input);
        }
        else if (tracker.IsKeyReleased(keyboardKey))
        {
            KeyboardInput input = {
                key,
                KeyboardInputType::Released
            };

            unsigned int index;

            if (State->pressedKeys->IndexOf(key, &index)) 
            {
                State->pressedKeys->RemoveAt(index);
            }

            Queue->Enqueue(input);
        }
    }
}

bool KeyboardInterlayer::IsKeyDown(Keys key)
{
    Keyboard::Keys keyboardKey = static_cast<Keyboard::Keys>(key);

    if (keyboardKey >= 0 && keyboardKey <= 0xfe)
    {
        auto ptr = reinterpret_cast<const uint32_t*>(&tracker.lastState);
        unsigned int bf = 1u << (keyboardKey & 0x1f);
        return (ptr[(keyboardKey >> 5)] & bf) != 0;
    }
    return false;
}

bool KeyboardInterlayer::IsKeyUp(Keys key)
{
    Keyboard::Keys keyboardKey = static_cast<Keyboard::Keys>(key);

    if (keyboardKey >= 0 && keyboardKey <= 0xfe)
    {
        auto ptr = reinterpret_cast<const uint32_t*>(&tracker.lastState);
        unsigned int bf = 1u << (keyboardKey & 0x1f);
        return (ptr[(keyboardKey >> 5)] & bf) == 0;
    }
    return false;
}