using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine;

public partial class TyzeWindow : GameWindow
{
    private readonly IReadOnlyList<IScene> _scenes;
    private int _currentSceneIndex;
    private Shader _shader;
    private Matrix4 _view, _projection;
    private List<IScript> _scripts;

    public TriggerHandler TriggerLoadObjects { get; }
    public TriggerHandler TriggerNextScene { get; }

    public TyzeWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, 
        IReadOnlyList<IScene> scenes, List<IScript> scripts = null) 
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
        _scripts = scripts ?? new List<IScript>();
        
        // SHOW FPS настройки.
        _title = nativeWindowSettings.Title;
        _time = 0;
        _frames = 0;
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(.9f, .9f, .9f, 1.0f);

        _shader = new Shader(Constants.ShaderVertTexturePath, Constants.ShaderFragTexturePath);
        _shader.Enable();

        // _view = Matrix4.CreateTranslation(0, 0, 3);
        // _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), 
        //     Size.X / (float)Size.Y, .1f, 100);
        _view = Matrix4.Identity;
        _projection = Matrix4.Identity;

        LoadObjects();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        ShowFps(args.Time);
        _shader.Enable();
        DrawObjects();
        _shader.Disable();

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