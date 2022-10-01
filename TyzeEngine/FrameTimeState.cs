namespace TyzeEngine;

public static class FrameTimeState
{
    public static float RenderTime { get; internal set; }
    public static float UpdateTime { get; internal set; }
    public static float FixedTime { get; internal set; }
}