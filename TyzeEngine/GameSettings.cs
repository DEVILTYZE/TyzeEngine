using System;
using OpenTK.Windowing.Common;

namespace TyzeEngine;

public class GameSettings
{
    internal TyzeWindow Window { get; set; }
    
    /// <summary> Режим запуска. Значение по умолчанию — Debug. </summary>
    public RunMode RunMode { get; set; } = RunMode.Debug;

    public CursorState CursorState
    {
        get
        {
            if (Window is null)
                throw new Exception("Can't use this property while window is not created.");
            
            return Window.CursorState;
        }
        set
        {
            if (Window is null)
                throw new Exception("Can't use this property while window is not created.");
            
            Window.CursorState = value;
        }
    }

    public VSyncMode VSync
    {
        get
        {
            if (Window is null)
                throw new Exception("Can't use this property while window is not created.");
            
            return Window.VSync;
        }
        set
        {
            if (Window is null)
                throw new Exception("Can't use this property while window is not created.");
            
            Window.VSync = value;
        }
    }

    public bool AntiAliasing { get; set; } = false;
    public double FixedTime { get; set; } = Constants.FixedTimeLimit;
    public Saver Saver { get; set; } = new();

    internal GameSettings()
    {
    }
}