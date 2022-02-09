#include "pch.h"
#include "InputComponent/Enumerations.h"
#include "InputComponent/KeyboardInterlayer.h"
#include "InputComponent/MouseInterlayer.h"
#include "InputComponent/InputComponent.h"

using namespace Teeditor_Direct3DInterop;

InputComponent::InputComponent(Window^ window, UIElement^ uiElement)
{   
    Keyboard = ref new KeyboardInterlayer(window);
    Mouse = ref new MouseInterlayer(window, uiElement, Keyboard);
}


void InputComponent::Update()
{
    Keyboard->Update();
    Mouse->Update();
}