using OpenTK.Windowing.Common;

namespace TyzeEngine;

public class GameSettings
{
    private CursorState _cursorState;
    private VSyncMode _vSyncMode;
    private TyzeWindow _window;
    
    /// <summary>
    /// Режим запуска. Значение по умолчанию — Debug.
    /// </summary>
    public RunMode RunMode { get; set; } = RunMode.Debug;
    public bool AntiAliasing { get; set; } = false;
    public float FixedTime { get; set; } = Constants.FixedTimeLimit;
    public Saver Saver { get; set; } = new();
    
    public CursorState CursorState
    {
        get => _cursorState;
        set
        {
            _cursorState = value;
            SetCursorState(_cursorState);
        }
    }

    public VSyncMode VSync
    {
        get => _vSyncMode;
        set
        {
            _vSyncMode = value;
            SetVSync(_vSyncMode);
        }
    }

    internal void SetToWindow(TyzeWindow window)
    {
        _window = window;
        VSync = _vSyncMode;
        CursorState = _cursorState;
    }
    
    private void SetCursorState(CursorState state)
    {
        if (_window is not null && _window.Exists)
            _window.CursorState = state;
    }

    private void SetVSync(VSyncMode vSync)
    {
        if (_window is not null && _window.Exists)
            _window.VSync = vSync;
    }
}