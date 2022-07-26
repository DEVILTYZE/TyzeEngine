﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using TyzeEngine.Interfaces;

namespace TyzeEngine;

internal sealed class TyzeWindow : GameWindow
{
    private ArrayObject _vectorObject;
    private float _accumulator;
    
    // AUDIO
    private ALDevice _device;
    private ALContext _context;
    
    // SHOW FPS
    private double _time;
    private int _frames;
    private readonly string _title;

    // PROPERTIES
    internal TriggerHandler TriggerNextScene { get; }

    internal TyzeWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) 
        : base(gameWindowSettings, nativeWindowSettings)
    {
        // SHOW FPS настройки.
        _title = nativeWindowSettings.Title;
        Input.SetStates(KeyboardState, MouseState);
        Input.Size = Size;
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(Constants.DarkSpaceColor);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);

        Game.SetShaders();
        Game.CurrentScene.LoadSceneHandler = TriggerNextScene;
        Camera.Fov = 45;
        Camera.SetAspectRatio(Size.X / (float)Size.Y);
        InitializeAudio();
        Game.CurrentScene.Run();
        Game.FrameScripts.ToList().ForEach(script => script.InternalPrepare());
        
        // При отладке идёт отображение векторов скоростей, сил и т.п.
        if (Game.Settings.RunMode == RunMode.Debug)
            InitializeVectorObject();

        if (!Game.Settings.AntiAliasing) 
            return;
        
        GL.Enable(EnableCap.Multisample);
        GL.Hint(HintTarget.MultisampleFilterHintNv, HintMode.Nicest);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        FrameTimeState.RenderTime = (float)args.Time;
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        ShowFps(args.Time);
        var objects = Game.CurrentScene.GetCurrentGameObjects();
        StepPhysics(objects, (float)args.Time);
        DrawObjects(objects);

        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        FrameTimeState.UpdateTime = (float)args.Time;
        
#if DEBUG
        if (Input.IsDown(KeyCode.Escape))
        {
            Close();
            return;
        }
#endif
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < Game.FrameScripts.Count; i++)
            Game.FrameScripts[i].InternalExecute();

        // something...
    }

    protected override void OnResize(ResizeEventArgs args)
    {
        base.OnResize(args);
        GL.Viewport(0, 0, Size.X, Size.Y);
        Input.Size = Size;
        Camera.SetAspectRatio(Size.X / (float)Size.Y);
    }

    protected override void OnUnload()
    {
        LoadQueue.Clear();
        ALC.MakeContextCurrent(ALContext.Null);
        ALC.DestroyContext(_context);
        ALC.CloseDevice(_device);
        base.OnUnload();
    }

    private void StepPhysics(IReadOnlyList<IGameObject> objects, float time)
    {
        _accumulator += time;

        if (_accumulator > Game.Settings.FixedTime)
            _accumulator = Game.Settings.FixedTime;

        while (_accumulator > FrameTimeState.FixedTime)
        {
            Game.PhysicsWorld.Step(objects);

            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < Game.FrameScripts.Count; ++i)
                Game.FrameScripts[i].InternalFixedExecute();

            _accumulator -= FrameTimeState.FixedTime;
        }
    }

    private void DrawObjects(IReadOnlyList<IGameObject> objects)
    {
        var lights = objects.Where(obj => obj.VisualType is BodyVisualType.Light)
            .Cast<ILightObject>().ToList();
        
        foreach (var obj in objects)
        {
            obj.Draw(lights);

            if (Game.Settings.RunMode != RunMode.Debug) 
                continue;
            
            obj.DrawLines();
            obj.Body?.GetVectors().ForEach(vector => DrawDebugVector(vector, obj));
        }
    }

    private void ShowFps(double time)
    {
        _time += time;
        ++_frames;

        if (_time < 1) 
            return;
        
        Title = _title + " / FPS: " + _frames;
        _time = 0;
        _frames = 0;
    }

    private void InitializeAudio()
    {
        var attribute = 1;
        _device = ALC.OpenDevice(null);
        _context = ALC.CreateContext(_device, ref attribute);
        ALC.MakeContextCurrent(_context);
    
        var version = AL.Get(ALGetString.Version);
        var vendor = AL.Get(ALGetString.Vendor);
        var renderer = AL.Get(ALGetString.Renderer);
        Console.WriteLine(version + "\r\n" + vendor + "\r\n" + renderer);
    }

    private void DrawDebugVector(Vector3 vector, IGameObject obj)
    {
        if (vector == Vector3.Zero)
            return;

        var shader = Game.Shaders[BodyVisualType.Line];
        shader.Enable();
        var scale = vector.LengthFast * Vector3.One;
        var position = obj.Transform.Position;
        _vectorObject.Enable();
        shader.SetMatrix4("model", GetMatrixParametrized(scale, Vector3.NormalizeFast(vector), position));
        shader.SetMatrix4("view", Camera.View);
        shader.SetMatrix4("projection", Camera.Projection);
        shader.SetVector4("inColor", new Vector4(1, 1, 1, 1));
        _vectorObject.Draw(PrimitiveType.LineLoop, 0, 2);
        _vectorObject.Disable();
        shader.Disable();
    }
    
    private void InitializeVectorObject()
    {
        if (_vectorObject is not null)
            return;
        
        var shader = Game.Shaders[BodyVisualType.Line];
        _vectorObject = new ArrayObject();
        _vectorObject.Enable();
        var startPosition = Vector.ToFloats(Vector3.Zero);
        var direction = Vector.ToFloats(Vector3.One);
        var vertices = startPosition.Concat(direction).ToArray();

        var arrayBuffer = new BufferObject(BufferTarget.ArrayBuffer);
        arrayBuffer.SetData(vertices, BufferUsageHint.StaticDraw);
        _vectorObject.AttachBuffer(arrayBuffer);
        var position = shader.GetAttributeLocation("aPosition");
        
        arrayBuffer.Enable();
        _vectorObject.EnableAttribute(position, Constants.Vector3Length, VertexAttribPointerType.Float, 
            Constants.Vector3Stride, 0);
        arrayBuffer.Disable();
    }
    
    private static Matrix4 GetMatrixParametrized(Vector3 scale, Vector3 rotation, Vector3 position)
    {
        var localScale = Matrix4.Identity * Matrix4.CreateScale(scale);
        var localRotation = localScale * Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(rotation));
        
        return localRotation * Matrix4.CreateTranslation(position);
    }
}