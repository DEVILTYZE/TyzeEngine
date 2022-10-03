namespace TyzeEngine;

public static class FrameTimeState
{
    public const float FixedTime = 1f / 144;
    
    public static float RenderTime { get; internal set; }
    public static float UpdateTime { get; internal set; }
}