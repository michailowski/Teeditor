#include "pch.h"
#include "InputComponent/MouseInterlayer.h"

using namespace Teeditor_Direct3DInterop;

MouseInterlayer::MouseInterlayer(Window^ window, UIElement^ uiElement, KeyboardInterlayer^ keyboard)
{
    mouse = std::make_unique<Mouse>();

    Queue = ref new MouseInputQueue();
    State = ref new MouseState();

    currentWindow = window;
    relativeContainer = uiElement;
    this->keyboard = keyboard;

    relativeContainer->PointerEntered += ref new PointerEventHandler(
        this,
        &MouseInterlayer::RelativeContainerPointerEntered
    );

    relativeContainer->PointerExited += ref new PointerEventHandler(
        this,
        &MouseInterlayer::RelativeContainerPointerExited
    );

    mouse->SetWindow(window->CoreWindow);

    auto currentDisplayInformation = DisplayInformation::GetForCurrentView();
    currentDisplayInformation->DpiChanged +=
        ref new TypedEventHandler<DisplayInformation^, Object^>(this, &MouseInterlayer::OnDpiChanged);

    mouse->SetDpi(currentDisplayInformation->LogicalDpi);
}

void MouseInterlayer::RelativeContainerPointerEntered(Object^ sender, PointerRoutedEventArgs^ args)
{
    isRelativeContainerPointed = true;
}

void MouseInterlayer::RelativeContainerPointerExited(Object^ sender, PointerRoutedEventArgs^ args)
{
    isRelativeContainerPointed = false;
}

void MouseInterlayer::OnDpiChanged(DisplayInformation^ sender, Object^ args)
{
    Mouse::SetDpi(sender->LogicalDpi);
}

void MouseInterlayer::Update()
{
    Queue->Clear();

    UpdateMouseState();

    if (!isRelativeContainerPointed)
        return;

    UpdateKeyModifiers();
    UpdateMouseInputs();
}

void MouseInterlayer::UpdateMouseState()
{
    auto rawState = mouse->GetState();
    tracker.Update(rawState);

    absolutePosition = float2((float)rawState.x, (float)rawState.y);

    ProcessRelativeMousePosition();

    MouseButtonState leftButtonState = {
        rawState.leftButton,
        tracker.leftButton == ButtonState::PRESSED,
        tracker.leftButton == ButtonState::RELEASED,
        tracker.leftButton == ButtonState::UP,
        tracker.leftButton == ButtonState::HELD
    };

    MouseButtonState middleButtonState = {
        rawState.middleButton,
        tracker.middleButton == ButtonState::PRESSED,
        tracker.middleButton == ButtonState::RELEASED,
        tracker.middleButton == ButtonState::UP,
        tracker.middleButton == ButtonState::HELD
    };

    MouseButtonState rightButtonState = {
        rawState.rightButton,
        tracker.rightButton == ButtonState::PRESSED,
        tracker.rightButton == ButtonState::RELEASED,
        tracker.rightButton == ButtonState::UP,
        tracker.rightButton == ButtonState::HELD
    };

    State->position = relativePosition;
    State->leftButton = leftButtonState;
    State->middleButton = middleButtonState;
    State->rightButton = rightButtonState;
    State->wheelDelta = rawState.scrollWheelValue;
}

void MouseInterlayer::UpdateKeyModifiers()
{
    ProcessKeyModifier(keyboard->IsKeyDown(Keys::LeftControl) || keyboard->IsKeyDown(Keys::RightControl), MouseKeyModifiers::Control);
    ProcessKeyModifier(keyboard->IsKeyDown(Keys::LeftAlt) || keyboard->IsKeyDown(Keys::RightAlt), MouseKeyModifiers::Alt);
    ProcessKeyModifier(keyboard->IsKeyDown(Keys::LeftShift) || keyboard->IsKeyDown(Keys::RightShift), MouseKeyModifiers::Shift);

    ProcessKeyModifier(State->LeftButton.IsPressed, MouseKeyModifiers::Left);
    ProcessKeyModifier(State->MiddleButton.IsPressed, MouseKeyModifiers::Middle);
    ProcessKeyModifier(State->RightButton.IsPressed, MouseKeyModifiers::Right);
}

void MouseInterlayer::ProcessKeyModifier(bool condition, MouseKeyModifiers keyModifier)
{
    if (condition)
        currentKeyModifiers |= keyModifier;
    else
        currentKeyModifiers &= ~keyModifier;
}

void MouseInterlayer::UpdateMouseInputs()
{
    ProcessMouseMove();

    ProcessMouseButton(MouseButton::Left, State->LeftButton);
    ProcessMouseButton(MouseButton::Middle, State->MiddleButton);
    ProcessMouseButton(MouseButton::Right, State->RightButton);

    ProcessMouseWheel();
}

void MouseInterlayer::ProcessRelativeMousePosition()
{
    currentWindow->Dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler([this]()
        {
            auto transform = currentWindow->Content->TransformToVisual(relativeContainer);

            Point absolutePoint = Point(absolutePosition.x, absolutePosition.y);
            Point relativePoint;

            if (transform->TryTransform(absolutePoint, &relativePoint))
                relativePosition = float2((float)relativePoint.X, (float)relativePoint.Y);
        }));
}

void MouseInterlayer::ProcessMouseMove()
{
    if (State->Position != prevRelativePosition)
        EnqueueMouseInput(MouseButton::None, MouseInputType::Move);

    prevRelativePosition = State->Position;
}

void MouseInterlayer::ProcessMouseButton(MouseButton button, MouseButtonState buttonState)
{
    if (buttonState.IsPressed && buttonState.WasPressed)
    {
        EnqueueMouseInput(button, MouseInputType::Click);
        EnqueueMouseInput(button, MouseInputType::Pressed);
    }
    else if (!buttonState.IsPressed && buttonState.WasReleased)
    {
        EnqueueMouseInput(button, MouseInputType::Released);
    }
}

void MouseInterlayer::ProcessMouseWheel()
{
    if (State->WheelDelta > 0)
        EnqueueMouseInput(MouseButton::None, MouseInputType::WheelUp);
    else if (State->WheelDelta < 0)
        EnqueueMouseInput(MouseButton::None, MouseInputType::WheelDown);

    mouse->ResetScrollWheelValue();
}

void MouseInterlayer::EnqueueMouseInput(MouseButton button, MouseInputType inputType)
{
    MouseInput input = {
        State->Position,
        button,
        inputType,
        currentKeyModifiers
    };

    Queue->Enqueue(input);
}