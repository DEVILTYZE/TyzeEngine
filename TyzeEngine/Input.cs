using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TyzeEngine;

public static class Input
{
    private const int Min = (int)KeyCode.Space;
    private const int Max = (int)KeyCode.RAlt;
    private static readonly SortedList<KeyCode, int> Keys = new();
    private static readonly SortedList<ButtonCode, int> Buttons = new();
    private static KeyboardState _keyboard;
    private static MouseState _mouse;

    #region Mouse

    public static bool IsAnyDown => _keyboard.IsAnyKeyDown || _mouse.IsAnyButtonDown;
    public static Vector2 Scroll => _mouse.Scroll;
    public static Vector2 ScrollDelta => _mouse.ScrollDelta;
    public static Vector2 MousePosition => new(_mouse.X - Size.X / 2, -(_mouse.Y - Size.Y / 2));
    public static Vector2 MouseDelta => new(_mouse.Delta.X, _mouse.Delta.Y);
    public static bool IsMouseUp => MousePosition.Y >= 0;
    public static bool IsMouseDown => MousePosition.Y < 0;
    public static bool IsMouseRight => MousePosition.X >= 0;
    public static bool IsMouseLeft => MousePosition.X < 0;
    public static float DeltaLimit { get; set; } = .5f;
    public static bool IsMouseUpNow => MouseDelta.Y < -DeltaLimit;
    public static bool IsMouseDownNow => MouseDelta.Y >= DeltaLimit;
    public static bool IsMouseRightNow => MouseDelta.X >= DeltaLimit;
    public static bool IsMouseLeftNow => MouseDelta.X < -DeltaLimit;

    #endregion
    
    internal static Vector2 Size { get; set; }

    static Input()
    {
        for (var i = Min; i <= Max; ++i)
            if (Enum.IsDefined(typeof(KeyCode), i))
                Keys.Add((KeyCode)i, i);

        for (var i = 0; i < 8; ++i)
            Buttons.Add((ButtonCode)i, i);
    }

    public static bool IsDown(KeyCode key) => _keyboard.IsKeyDown((Keys)Keys[key]);

    public static bool IsUp(KeyCode key) => _keyboard.IsKeyReleased((Keys)Keys[key]);

    public static bool IsDown(ButtonCode button) => _mouse.IsButtonDown((MouseButton)Buttons[button]);
    
    public static bool IsUp(ButtonCode button) => _mouse.IsButtonReleased((MouseButton)Buttons[button]);

    // TODO: добавить загрузку данных из сохранения.
    public static void Change(KeyCode oldKey, KeyCode newKey) => Keys[oldKey] = (int)newKey;

    public static void Change(ButtonCode oldButton, ButtonCode newButton) => Buttons[oldButton] = (int)newButton;

    public static void ToDefault()
    {
        foreach (var keyCode in Keys.Keys)
            Keys[keyCode] = (int)keyCode;

        foreach (var buttonCode in Buttons.Keys)
            Buttons[buttonCode] = (int)buttonCode;
    }

    internal static void SetStates(KeyboardState keyboard, MouseState mouse)
    {
        _keyboard = keyboard;
        _mouse = mouse;
    }
}