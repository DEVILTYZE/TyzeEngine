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
    private ISpace CurrentPlace => CurrentScene.CurrentPlace;

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
        VSync = VSyncMode.Off;
        
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
        GL.CullFace(CullFaceMode.Back);
        
        CurrentScene.ReloadObjects = TriggerLoadObjects;
        CurrentScene.LoadSceneHandler = TriggerNextScene;
        CursorState = CursorState.Grabbed;
        
        Game.SetShaders();
        Camera.Fov = 45;
        Camera.SetAspectRatio(Size.X / (float)Size.Y);

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
        DrawObjects();

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
        Camera.SetAspectRatio(Size.X / (float)Size.Y);
    }

    protected override void OnUnload()
    {
        LoadQueue.Clear();
        ALC.MakeContextCurrent(ALContext.Null);
        ALC.DestroyContext(_context);
        ALC.CloseDevice(_device);

        foreach (var (_, shader) in Game.Shaders)
            shader.Dispose();
        
        foreach (var scene in Scenes)
            scene.Dispose();

        base.OnUnload();
    }
    
    private void LoadObjects()
    {
        // При отладке идёт отображение векторов скоростей, сил и т.п.
        if (IsDebugMode)
            InitializeVectorObject();
        
        // Загрузка объекта.
        foreach (var obj in GetNewObjects())
            obj.Load();
    }

    private void DrawObjects()
    {
        var objects = new[] { CurrentPlace }.Concat(CurrentPlace.NeighbourSpaces).SelectMany(place => 
            place.GameObjects).ToList();
        var objectList = objects.Where(obj => obj.Visual.Visibility is Visibility.Visible).ToList();
        var lights = objectList.Where(obj => obj.Visual.Type is BodyVisualType.Light).ToList();
        
        foreach (var obj in objectList)
        {
            obj.Draw(lights);

            if (obj.Body is null)
                continue;
            
            foreach (var vector in obj.Body.GetVectors()!)
                DrawDebugVector(vector, obj);
        }

        objectList = objects.Where(obj => 
            obj.Body is not null && 
            obj.Visual.Visibility is not Visibility.Collapsed && 
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

        var shader = Game.Shaders[BodyVisualType.Color];
        var scale = vector / Vector3.One;
        var axis = Vector3.Cross(Vector3.One, vector);
        var angle = Vector3.CalculateAngle(Vector3.One, vector);
        var position = obj.Transform.Position;
        _vectorObject.Enable();
        shader.SetMatrix4("model", GetMatrixParametrized(scale, axis * angle, position));
        shader.SetMatrix4("view", Camera.View);
        shader.SetMatrix4("projection", Camera.Projection);
        _vectorObject.Draw(PrimitiveType.LineLoop, 0, 2);
        _vectorObject.Disable();
    }
    
    private void InitializeVectorObject()
    {
        if (_vectorObject is not null)
            return;
        
        var shader = Game.Shaders[BodyVisualType.Color];
        _vectorObject = new ArrayObject();
        _vectorObject.Enable();
        var startPosition = Vector.ToFloats(Vector3.Zero);
        var direction = Vector.ToFloats(Vector3.One);
        var color = new Vector4(.5f, .1f, .2f, 1);
        var colorFloats = Vector.ToFloats(color).ToArray();
        var temp = new float[] { -1, -1 };
        var vertices = startPosition.Concat(colorFloats).Concat(temp).Concat(direction).Concat(colorFloats).Concat(temp).ToArray();

        var arrayBuffer = new BufferObject(BufferTarget.ArrayBuffer);
        arrayBuffer.SetData(vertices, BufferUsageHint.StaticDraw);
        _vectorObject.AttachBuffer(arrayBuffer);
        
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