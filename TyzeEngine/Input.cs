using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TyzeEngine;

public static class Input
{
    private const int Min = (int)KeyCode.Space;
    private const int Max = (int)KeyCode.RAlt;
    private static readonly SortedList<KeyCode, int> KeyboardKeys = new();
    private static readonly SortedList<ButtonCode, int> MouseButtons = new();
    private static KeyboardState _keyboard;
    private static MouseState _mouse;

    public static bool IsAnyKeyOrButtonDown => _keyboard.IsAnyKeyDown || _mouse.IsAnyButtonDown;
    public static bool IsAnyKeyDown => _keyboard.IsAnyKeyDown;
    public static bool IsAnyButtonDown => _mouse.IsAnyButtonDown;
    public static Vector2 Scroll => _mouse.Scroll;
    public static Vector2 ScrollDelta => _mouse.ScrollDelta;
    public static Vector2 MousePosition => new(_mouse.X - Size.X / 2, -(_mouse.Y - Size.Y / 2));
    public static bool IsMouseUp => MousePosition.Y >= 0;
    public static bool IsMouseDown => MousePosition.Y < 0;
    public static bool IsMouseRight => MousePosition.X >= 0;
    public static bool IsMouseLeft => MousePosition.X < 0;

    internal static Vector2 Size { get; set; }

    static Input()
    {
        for (var i = Min; i <= Max; ++i)
            if (Enum.IsDefined(typeof(KeyCode), i))
                KeyboardKeys.Add((KeyCode)i, i);

        for (var i = 0; i < 8; ++i)
            MouseButtons.Add((ButtonCode)i, i);
    }

    public static bool IsDown(KeyCode key) => _keyboard.IsKeyDown((Keys)KeyboardKeys[key]);

    public static bool IsUp(KeyCode key) => _keyboard.IsKeyReleased((Keys)KeyboardKeys[key]);

    public static bool IsDown(ButtonCode button) => _mouse.IsButtonDown((MouseButton)MouseButtons[button]);
    
    public static bool IsUp(ButtonCode button) => _mouse.IsButtonReleased((MouseButton)MouseButtons[button]);

    // TODO: добавить загрузку данных из сохранения.
    public static void Change(KeyCode oldKey, KeyCode newKey) => 
        (KeyboardKeys[oldKey], KeyboardKeys[newKey]) = ((int)newKey, (int)oldKey);

    public static void Change(ButtonCode oldButton, ButtonCode newButton) =>
        (MouseButtons[oldButton], MouseButtons[newButton]) = ((int)newButton, (int)oldButton);

    public static void ToDefault()
    {
        KeyboardKeys.Keys.ToList().ForEach(keyCode => KeyboardKeys[keyCode] = (int)keyCode);
        MouseButtons.Keys.ToList().ForEach(buttonCode => MouseButtons[buttonCode] = (int)buttonCode);
    }

    internal static void SetStates(KeyboardState keyboard, MouseState mouse)
    {
        _keyboard = keyboard;
        _mouse = mouse;
    }
}