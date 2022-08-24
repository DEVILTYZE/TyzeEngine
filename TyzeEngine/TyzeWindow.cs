using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;

namespace TyzeEngine;

internal sealed class TyzeWindow : GameWindow
{
    private Shader _shader;
    private Matrix4 _view, _projection;
    
    // AUDIO
    private ALDevice _device;
    private ALContext _context;
    
    // FPS
    private double _time;
    private int _frames;
    private readonly string _title;

    // PROPERTIES
    private IScene CurrentScene => Scenes?[CurrentSceneIndex];
    private IPlace CurrentPlace => CurrentScene.CurrentPlace;

    internal int CurrentSceneIndex { get; set; }
    internal IReadOnlyList<IScene> Scenes { get; set; }
    internal IReadOnlyList<IScript> Scripts { get; set; }

    internal TriggerHandler TriggerLoadObjects { get; }
    internal TriggerHandler TriggerNextScene { get; }
    internal bool IsDebugMode { get; set; }

    internal TyzeWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) 
        : base(gameWindowSettings, nativeWindowSettings)
    {
        TriggerLoadObjects += LoadObjects;
        TriggerNextScene += LoadScene;
        VSync = VSyncMode.Off;

        // SHOW FPS настройки.
        _title = nativeWindowSettings.Title;
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(.9f, .9f, .9f, .1f);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Back);
        
        CurrentScene.ReloadObjects = TriggerLoadObjects;
        CurrentScene.LoadSceneHandler = TriggerNextScene;
        
        foreach (var script in Scripts)
            script.SetKeyboardState(KeyboardState);
        
        InitializeAudio();
        _shader = new Shader(Constants.ShaderVertTexturePath, Constants.ShaderFragTexturePath);
        _shader.Enable();

        _view = Matrix4.CreateTranslation(0, 0, -10);
        _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), 
            Size.X / (float)Size.Y, .1f, 100);

        CurrentScene.Run();
        LoadObjects(new TriggeredEventArgs(false));
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        ShowFps(args.Time);
        _shader.Enable();
        DrawObjects((float)args.Time);
        _shader.Disable();

        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();

        foreach (var script in Scripts)
            script.Execute(new TriggeredEventArgs(args.Time));

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
        LoadQueue.Clear();
        CurrentPlace?.Dispose();
        ALC.MakeContextCurrent(ALContext.Null);
        ALC.DestroyContext(_context);
        ALC.CloseDevice(_device);

        base.OnUnload();
    }
    
    private void LoadObjects(TriggeredEventArgs args)
    {
        var objects = args is not null && (bool)args.Data ? GetNewObjects() : GetCurrentObjects();
        const int stride = Constants.Vector3Stride + Constants.Vector2Stride + Constants.Vector4Stride;
        
        // Загрузка объекта.
        foreach (var obj in objects)
        {
            // Создание нового Array object для каждого игрового объекта.
            obj.ArrayObject = new ArrayObject();

            if (IsDebugMode)
                ArrayObject.Primitive = PrimitiveType.LineLoop;

            if (obj.Body?.Visibility is VisibilityType.Collapsed or VisibilityType.Hidden)
                continue;
            
            obj.ArrayObject.Enable();
            // Получение точек позиции объекта в пространстве, текстуры в пространстве и цвета в виде массива float
            // и получение массива uint для Element object.
            var vertices = obj.Model.GetVectorArray(obj);

            // Создание буферa для векторного представления.
            var buffer = new BufferObject(BufferTarget.ArrayBuffer);
            buffer.SetData(vertices.Item1, obj.DrawType);
            obj.ArrayObject.AttachBuffer(buffer, 0);
            
            // Получение индексов для трёх атрибутов (позиция, текстура и цвет).
            var position = _shader.GetAttributeLocation("aPosition");
            var texture = _shader.GetAttributeLocation("inTexture");
            var color = _shader.GetAttributeLocation("inColor");
            
            // Настройка атрибутов.
            buffer.Enable();
            obj.ArrayObject.EnableAttribute(position, Constants.Vector3Length, VertexAttribPointerType.Float, 
                stride, 0);
            obj.ArrayObject.EnableAttribute(texture, Constants.Vector2Length, VertexAttribPointerType.Float, 
                stride, Constants.Vector3Stride);
            obj.ArrayObject.EnableAttribute(color, Constants.Vector4Length, VertexAttribPointerType.Float, 
                stride, Constants.Vector3Stride + Constants.Vector2Stride);
            buffer.Disable();

            // Создание буфера для Element object.
            var indicesBuffer = new BufferObject(BufferTarget.ElementArrayBuffer);
            indicesBuffer.SetData(vertices.Item2, obj.DrawType);
            obj.ArrayObject.AttachBuffer(indicesBuffer, vertices.Item2.Length);
            
            obj.ArrayObject.Disable();
            // Связывание ресурсов для текущего объекта.
            obj.Texture?.Enable();
        }
    }

    private void DrawObjects(float time)
    {
        Matrix4 GetMatrix(IBody body)
        {
            // SCALE
            var scale = Matrix4.Identity * Matrix4.CreateScale(body.Scale);
            
            // ROTATION
            var angularAcceleration = body.Torque * body.InverseInertia;
            body.AngularVelocity += angularAcceleration * time;
            body.Rotation += body.AngularVelocity * time;
            var rotationX = scale * Matrix4.CreateRotationX(body.Rotation.X);
            var rotationY = rotationX * Matrix4.CreateRotationY(body.Rotation.Y);
            var rotationZ = rotationY * Matrix4.CreateRotationZ(body.Rotation.Z);
            
            // TRANSLATION
            var acceleration = body.Force * body.InverseMass + body.GravityForce;
            body.Velocity += acceleration * time;
            body.Position += body.Velocity * time;
            
            return rotationZ * Matrix4.CreateTranslation(body.Position);
        }
        
        var objects = GetCurrentObjects().ToList();
        
        foreach (var obj in objects)
        {
            obj.ArrayObject.Enable();
            obj.Texture?.Enable();
            _shader.SetMatrix4("model", GetMatrix(obj.Body));
            _shader.SetMatrix4("view", _view);
            _shader.SetMatrix4("projection", _projection);
            obj.ArrayObject.Draw(DrawElementsType.UnsignedInt);
            obj.Texture?.Disable();
        }

        objects = objects.Where(obj => obj.Body is not null && obj.Body.IsEnabled).ToList();
        PhysicsGenerator.DetermineCollision(objects);
    }

    private void LoadScene(TriggeredEventArgs args) => CurrentSceneIndex = (int)args.Data;

    private IEnumerable<IGameObject> GetCurrentObjects()
        => new[] { CurrentPlace }.Concat(CurrentPlace.NeighbourPlaces).SelectMany(place => place.GameObjects);

    private IEnumerable<IGameObject> GetNewObjects()
    {
        var objects = LoadQueue.TakeObjects().ToArray();
        CurrentPlace.GameObjects.AddRange(objects);
        CurrentScene.LoadResources();

        return objects;
    }

    private void ShowFps(double time)
    {
        _time += time;
        ++_frames;

        if (_time < 1.0) 
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
}