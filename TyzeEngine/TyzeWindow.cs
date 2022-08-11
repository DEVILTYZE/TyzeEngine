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

public sealed class TyzeWindow : GameWindow
{
    private readonly IReadOnlyList<IScene> _scenes;
    private int _currentSceneIndex;
    private readonly IReadOnlyList<IScript> _scripts;
    private Shader _shader;
    private Matrix4 _view, _projection;
    private ALDevice _device;
    private ALContext _context;
    private double _time;
    private int _frames;
    private readonly string _title;

    public TriggerHandler TriggerLoadObjects { get; }
    public TriggerHandler TriggerNextScene { get; }

    public TyzeWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, 
        IReadOnlyList<IScene> scenes, IReadOnlyList<IScript> scripts = null) 
        : base(gameWindowSettings, nativeWindowSettings)
    {
        if (scenes is null || scenes.Count == 0)
            throw new ArgumentException("Count of scenes is zero.", nameof(scenes));
        
        TriggerLoadObjects += LoadObjects;
        TriggerNextScene += LoadScene;
        VSync = VSyncMode.On;
        _scenes = scenes;
        _currentSceneIndex = 0;
        _scenes[_currentSceneIndex].ReloadObjects = TriggerLoadObjects;
        _scenes[_currentSceneIndex].LoadSceneHandler = TriggerNextScene;
        _scripts = scripts ?? new List<IScript>();

        foreach (var script in _scripts)
            script.AddArgs(KeyboardState, _scenes[_currentSceneIndex]);
        
        // SHOW FPS настройки.
        _title = nativeWindowSettings.Title;
        _time = 0;
        _frames = 0;
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(.9f, .9f, .9f, .1f);
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Back);

        InitializeAudio();
        _shader = new Shader(Constants.ShaderVertTexturePath, Constants.ShaderFragTexturePath);
        _shader.Enable();

        _view = Matrix4.CreateTranslation(0, 0, -10);
        _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), 
            Size.X / (float)Size.Y, .1f, 100);

        _scenes[_currentSceneIndex].Run();
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

        foreach (var script in _scripts)
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
        
        foreach (var place in _scenes[_currentSceneIndex].CurrentPlace.NeighbourPlaces)
            ((IDisposable)place).Dispose();

        ((IDisposable)_scenes[_currentSceneIndex].CurrentPlace).Dispose();
        ALC.MakeContextCurrent(ALContext.Null);
        ALC.DestroyContext(_context);
        ALC.CloseDevice(_device);

        base.OnUnload();
    }
    
    private void LoadObjects(TriggeredEventArgs args)
    {
        var objects = GetObjects(args is not null && (bool)args.Data);
        const int stride = Constants.VertexStride + Constants.TextureStride + Constants.ColorStride;
        
        // Загрузка объекта.
        foreach (var obj in objects)
        {
            // Создание нового Array object для каждого игрового объекта.
            obj.ArrayObject = new ArrayObject();
            
            if (obj.Body.Visibility is VisibilityType.Collapsed or VisibilityType.Hidden)
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
            
            buffer.Enable();
            obj.ArrayObject.EnableAttribute(position, Constants.VertexLength, VertexAttribPointerType.Float, 
                stride, 0);
            obj.ArrayObject.EnableAttribute(texture, Constants.TextureLength, VertexAttribPointerType.Float, 
                stride, Constants.VertexStride);
            obj.ArrayObject.EnableAttribute(color, Constants.ColorLength, VertexAttribPointerType.Float, 
                stride, Constants.VertexStride + Constants.TextureStride);
            buffer.Disable();

            // Создание буфера для Element object.
            var indicesBuffer = new BufferObject(BufferTarget.ElementArrayBuffer);
            indicesBuffer.SetData(vertices.Item2, obj.DrawType);
            obj.ArrayObject.AttachBuffer(indicesBuffer, vertices.Item2.Length);
            
            obj.ArrayObject.Disable();
            // Связывание ресурсов для текущего объекта.
            foreach (var resourceId in obj.ResourceIds)
                _scenes[_currentSceneIndex].EnableResource(resourceId);
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
        void SetMatrices(IGameObject obj)
        {
            var model = GetMatrix(obj.Body);
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _view);
            _shader.SetMatrix4("projection", _projection);
        }
        
        var objects = GetObjects(false).ToList();
        
        foreach (var obj in objects)
        {
            obj.ArrayObject.Enable();
            _scenes[_currentSceneIndex].EnableResource(obj.ResourceId);
            SetMatrices(obj);
            obj.ArrayObject.Draw(DrawElementsType.UnsignedInt);
            _scenes[_currentSceneIndex].DisableResource(obj.ResourceId);
        }

        objects = objects.Where(obj => obj.Body is not null).ToList();
        PhysicsGenerator.DetermineCollision(objects);
    }

    private void LoadScene(TriggeredEventArgs args) => _currentSceneIndex = (int)args.Data;

    private IEnumerable<IGameObject> GetObjects(bool unloaded)
    {
        var currentPlace = _scenes[_currentSceneIndex].CurrentPlace;

        if (!unloaded)
            return new[] { currentPlace }.Concat(currentPlace.NeighbourPlaces).SelectMany(place => place.GameObjects);

        var objects = LoadQueue.TakeObjects().ToArray();
        currentPlace.GameObjects.AddRange(objects);
        _scenes[_currentSceneIndex].LoadResources();

        return objects;
    }

    private void ShowFps(double time)
    {
        _time += time;
        ++_frames;

        if (_time < 1.0) 
            return;
        
        Title = _title + " / FPS: " + _frames;
        _time = 0.0;
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