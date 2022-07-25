using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TyzeEngine.GameStructure;
using TyzeEngine.Objects;

namespace TyzeEngine;

public partial class TyzeWindow : GameWindow
{
    private readonly IReadOnlyList<IScene> _scenes;
    private int _currentSceneIndex;
    private Shader _shader;

    public TriggerHandler TriggerLoadObjects { get; }
    public TriggerHandler TriggerNextScene { get; }

    public TyzeWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, 
        IReadOnlyList<IScene> scenes) 
        : base(gameWindowSettings, nativeWindowSettings)
    {
        if (scenes is null || scenes.Count == 0)
            throw new ArgumentException("Count of scenes is zero.", nameof(scenes));

        TriggerLoadObjects += LoadObjects;
        TriggerNextScene += LoadScene;
        VSync = VSyncMode.Off;
        _scenes = scenes;
        _currentSceneIndex = 0;
        _scenes[_currentSceneIndex].ReloadObjects = TriggerLoadObjects;
        _scenes[_currentSceneIndex].LoadSceneHandler = TriggerNextScene;
        
        // SHOW FPS SETTINGS
        _title = nativeWindowSettings.Title;
        _time = 0;
        _frames = 0;
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(.9f, .9f, .9f, 1.0f);

        _shader = new Shader(ConstHelper.ShaderVertTexturePath, ConstHelper.ShaderFragTexturePath);
        UseShader(true);

        LoadObjects();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        ShowFps(args.Time);
        UseShader(true);
        DrawObjects();
        UseShader(false);

        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();

        // something...
    }

    protected override void OnResize(ResizeEventArgs args)
    {
        base.OnResize(args);
        GL.Viewport(0, 0, Size.X, Size.Y);
    }

    protected override void OnUnload()
    {
        _shader.Dispose();
        
        foreach (var place in _scenes[_currentSceneIndex].CurrentPlace.NeighbourPlaces)
            ((IDisposable)place).Dispose();

        ((IDisposable)_scenes[_currentSceneIndex].CurrentPlace).Dispose();

        base.OnUnload();
    }
}