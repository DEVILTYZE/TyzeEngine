using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine;

internal sealed class TyzeWindow : GameWindow
{
    private Shader _shader;
    private ArrayObject _vectorObject;
    
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
        VSync = VSyncMode.On;

        // SHOW FPS настройки.
        _title = nativeWindowSettings.Title;
        Input.SetStates(KeyboardState, MouseState);
        Input.Size = Size;
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
        CursorState = CursorState.Grabbed;
        
        _shader = new Shader(Constants.ShaderVertTexturePath, Constants.ShaderFragTexturePath);
        _shader.Enable();
        
        Camera.Instance.Fov = 45;
        Camera.Instance.AspectRatio = Size.X / (float)Size.Y;

        InitializeAudio();
        CurrentScene.Run();
        LoadObjects();
        
        foreach (var script in Scripts)
            script.Prepare();
        
        Game.StartFixedExecute();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        FrameTimeState.RenderTime = (float)args.Time;
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
        FrameTimeState.UpdateTime = (float)args.Time;
        
#if DEBUG
        if (Input.IsDown(KeyCode.Escape))
            Close();
#endif
        
        foreach (var script in Scripts)
            script.Execute();

        // something...
    }

    protected override void OnResize(ResizeEventArgs args)
    {
        base.OnResize(args);
        GL.Viewport(0, 0, Size.X, Size.Y);
        Input.Size = Size;
    }

    protected override void OnUnload()
    {
        _shader.Dispose();
        LoadQueue.Clear();
        ALC.MakeContextCurrent(ALContext.Null);
        ALC.DestroyContext(_context);
        ALC.CloseDevice(_device);

        foreach (var scene in Scenes)
            scene.Dispose();

        base.OnUnload();
    }
    
    private void LoadObjects()
    {
        // Загрузка объекта.
        foreach (var obj in GetNewObjects())
        {
            // При отладке идёт отображение векторов скоростей, сил и т.п.
            if (IsDebugMode)
                InitializeVectorObject(_shader);
            
            obj.Load(_shader);
        }
    }

    private void DrawObjects(float time)
    {
        Matrix4 GetMatrix(IGameObject obj)
        {
            if (obj.Body is null)
                return GetMatrixParametrized(obj.Transform.Scale, obj.Transform.Rotation, obj.Transform.Position);

            // SCALE
            var scale = Matrix4.Identity * Matrix4.CreateScale(obj.Transform.Scale);
            
            // ROTATION
            var angularAcceleration = obj.Body.Torque * obj.Body.InverseInertia;
            obj.Body.AngularVelocity += angularAcceleration * time;
            obj.Transform.Rotation += obj.Body.AngularVelocity * time;
            var rotation = scale * Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(obj.Transform.Rotation));

            // TRANSLATION
            var acceleration = obj.Body.Force * obj.Body.InverseMass + obj.Body.GravityForce;
            obj.Body.Velocity += acceleration * time;
            obj.Transform.Position += obj.Body.Velocity * time;
            
            return rotation * Matrix4.CreateTranslation(obj.Transform.Position);
        }

        var objects = new[] { CurrentPlace }.Concat(CurrentPlace.NeighbourPlaces).SelectMany(place => 
            place.GameObjects).ToList();
        var objectList = objects.Where(obj => obj.Transform?.Visibility is Visibility.Visible).ToList();
        
        foreach (var obj in objectList)
        {
            obj.ArrayObject.Enable();
            obj.Transform.Texture?.Enable();
            _shader.SetMatrix4("model", GetMatrix(obj));
            _shader.SetMatrix4("view", Camera.Instance.View);
            _shader.SetMatrix4("projection", Camera.Instance.Projection);
            obj.ArrayObject.Draw(PrimitiveType.Triangles, DrawElementsType.UnsignedInt);
            obj.Transform.Texture?.Disable();
            obj.ArrayObject.Disable();

            if (obj.Body is null)
                continue;
            
            foreach (var vector in obj.Body.GetVectors()!)
                DrawDebugVector(vector, obj);
        }

        objectList = objects.Where(obj => 
            obj.Body is not null && 
            obj.Transform.Visibility is not Visibility.Collapsed && 
            obj.Body.IsEnabled).ToList();
        // PhysicsGenerator.DetermineCollision(objectList);
    }

    private IEnumerable<IGameObject> GetNewObjects()
    {
        var objects = LoadQueue.TakeObjects().ToArray();
        CurrentScene.LoadResources();

        return objects;
    }

    private void ShowFps(double time)
    {
        _time += time;
        ++_frames;

        if (_time < Constants.OneSecond) 
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
        
        var scale = vector / Vector3.One;
        var axis = Vector3.Cross(Vector3.One, vector);
        var angle = Vector3.CalculateAngle(Vector3.One, vector);
        var position = obj.Transform.Position;
        _vectorObject.Enable();
        _shader.SetMatrix4("model", GetMatrixParametrized(scale, axis * angle, position));
        _shader.SetMatrix4("view", Camera.Instance.View);
        _shader.SetMatrix4("projection", Camera.Instance.Projection);
        _vectorObject.Draw(PrimitiveType.LineLoop);
        _vectorObject.Disable();
    }
    
    private void InitializeVectorObject(Shader shader)
    {
        _vectorObject ??= new ArrayObject();
        _vectorObject.Enable();
        var startPosition = Vector.ToFloats(Vector3.Zero);
        var direction = Vector.ToFloats(Vector3.One);
        var color = new Vector4(.5f, .1f, .2f, 1);
        var colorFloats = Vector.ToFloats(color).ToArray();
        var temp = new float[] { -1, -1 };
        var vertices = startPosition.Concat(colorFloats).Concat(temp).Concat(direction).Concat(colorFloats).Concat(temp).ToArray();

        var arrayBuffer = new BufferObject(BufferTarget.ArrayBuffer);
        arrayBuffer.SetData(vertices, BufferUsageHint.StaticDraw);
        _vectorObject.AttachBuffer(arrayBuffer, 2);
        
        var position = shader.GetAttributeLocation("aPosition");
        var texture = shader.GetAttributeLocation("inTexture");
        var colorPos = shader.GetAttributeLocation("inColor");
        
        arrayBuffer.Enable();
        _vectorObject.EnableAttribute(position, Constants.Vector3Length, VertexAttribPointerType.Float, 
            Constants.VectorStride, 0);
        _vectorObject.EnableAttribute(texture, Constants.Vector2Length, VertexAttribPointerType.Float, 
            Constants.VectorStride, Constants.Vector3Stride);
        _vectorObject.EnableAttribute(colorPos, Constants.Vector4Length, VertexAttribPointerType.Float, 
            Constants.VectorStride, Constants.Vector3Stride + Constants.Vector2Stride);
        arrayBuffer.Disable();
    }
    
    private static Matrix4 GetMatrixParametrized(Vector3 scale, Vector3 rotation, Vector3 position)
    {
        var localScale = Matrix4.Identity * Matrix4.CreateScale(scale);
        var localRotation = localScale * Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(rotation));
        
        return localRotation * Matrix4.CreateTranslation(position);
    }
}