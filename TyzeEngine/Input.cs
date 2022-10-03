using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TyzeEngine;

/// <summary>
/// Класс, упрощающий работу с вводами с клавиатуры и мыши.
/// </summary>
public static class Input
{
    private const int Min = (int)KeyCode.Space;
    private const int Max = (int)KeyCode.RAlt;
    private static readonly SortedList<KeyCode, int> KeyboardKeys = new();
    private static readonly SortedList<ButtonCode, int> MouseButtons = new();
    private static KeyboardState _keyboard;
    private static MouseState _mouse;

    /// <summary>
    /// Истина, если была нажата любая клавиша клавиатуры или кнопка мыши.
    /// </summary>
    public static bool IsAnyKeyOrButtonDown => _keyboard.IsAnyKeyDown || _mouse.IsAnyButtonDown;
    
    /// <summary>
    /// Истина, если была нажата любая клавиша клавиатуры.
    /// </summary>
    public static bool IsAnyKeyDown => _keyboard.IsAnyKeyDown;
    
    /// <summary>
    /// Истина, если была нажата любая кнопка мыши.
    /// </summary>
    public static bool IsAnyButtonDown => _mouse.IsAnyButtonDown;
    
    /// <summary>
    /// Значение прокрута колеса мыши, представленное в виде Vector2, где X — горизонтальный прокрут,
    /// а Y — вертикальный.
    /// </summary>
    public static Vector2 Scroll => _mouse.Scroll;
    
    /// <summary>
    /// Значение разницы между кадрами прокрута колеса мыши, представленное в виде Vector2, где X — горизонтальный
    /// прокрут, а Y — вертикальный.
    /// </summary>
    public static Vector2 ScrollDelta => _mouse.ScrollDelta;
    
    /// <summary>
    /// Значение позиции курсора мыши относительно центра окна.
    /// </summary>
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

    /// <summary>
    /// Метод проверяющий, зажата ли в данный момент определённая клавиша клавиатуры.
    /// </summary>
    /// <param name="key">Клавиша клавиатуры.</param>
    /// <returns>Истина, если зажата.</returns>
    public static bool IsDown(KeyCode key) => _keyboard.IsKeyDown((Keys)KeyboardKeys[key]);

    /// <summary>
    /// Метод проверяющий, отпущена ли в данный момент определённая клавиша клавиатуры.
    /// </summary>
    /// <param name="key">Клавиша клавиатуры.</param>
    /// <returns>Истина, если отпущена.</returns>
    public static bool IsUp(KeyCode key) => _keyboard.IsKeyReleased((Keys)KeyboardKeys[key]);
    
    /// <summary>
    /// Метод проверяющий, зажата ли в данный момент определённая кнопка мыши.
    /// </summary>
    /// <param name="button">Кнопка мыши.</param>
    /// <returns>Истина, если зажата.</returns>
    public static bool IsDown(ButtonCode button) => _mouse.IsButtonDown((MouseButton)MouseButtons[button]);
    
    /// <summary>
    /// Метод проверяющий, отпущена ли в данный момент определённая кнопка мыши.
    /// </summary>
    /// <param name="button">Кнопка мыши.</param>
    /// <returns>Истина, если отпущена.</returns>
    public static bool IsUp(ButtonCode button) => _mouse.IsButtonReleased((MouseButton)MouseButtons[button]);

    // TODO: добавить загрузку данных из сохранения.
    /// <summary>
    /// Метод, позволяющий сменить клавишу клавиатуры.
    /// </summary>
    /// <param name="oldKey">Старая клавиша клавиатуры.</param>
    /// <param name="newKey">Новая клавиша клавиатуры.</param>
    public static void Change(KeyCode oldKey, KeyCode newKey) => 
        (KeyboardKeys[oldKey], KeyboardKeys[newKey]) = ((int)newKey, (int)oldKey);

    /// <summary>
    /// Метод, позволяющий сменить кнопку мыши.
    /// </summary>
    /// <param name="oldButton">Старая кнопка мыши.</param>
    /// <param name="newButton">Новая кнопка мыши.</param>
    public static void Change(ButtonCode oldButton, ButtonCode newButton) =>
        (MouseButtons[oldButton], MouseButtons[newButton]) = ((int)newButton, (int)oldButton);

    /// <summary>
    /// Метод, возвращающий все значение клавиш и кнопок по умолчанию.
    /// </summary>
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