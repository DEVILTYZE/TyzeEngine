using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace TyzeEngine;

public class GameSettings
{
    private readonly GameWindowSettings _gameWindowSettings = new() { RenderFrequency = 144, UpdateFrequency = 144 };
    
    internal TyzeWindow Window { get; }

    /// <summary> Режим запуска. Значение по умолчанию — Debug. </summary>
    public RunMode RunMode { get; set; } = RunMode.Debug;
    public CursorState CursorState { get => Window.CursorState; set => Window.CursorState = value; }
    public VSyncMode VSync { get => Window.VSync; set => Window.VSync = value; }
    public bool AntiAliasing { get; set; } = false;
    public double FixedTime { get; set; } = Constants.FixedTimeLimit;
    public Saver Saver { get; set; } = new();
    
    internal GameSettings(NativeWindowSettings settings) => Window = new TyzeWindow(_gameWindowSettings, settings);
}