using OpenTK.Windowing.Common;

namespace TyzeEngine;

public class GameSettings
{
    internal TyzeWindow Window { get; set; }
    
    /// <summary> Режим запуска. Значение по умолчанию — Debug. </summary>
    public RunMode RunMode { get; set; } = RunMode.Debug;
    public Saver Saver { get; set; } = new();
    public double FixedTime { get; set; } = Constants.FixedTimeLimit;
    public CursorState CursorState { get => Window.CursorState; set => Window.CursorState = value; }
}