using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class GameObject : IGameObject
{
    private const int MaxChanges = 1;
    private bool _disposed;
    private ArrayObject _arrayObject;
    private IModel _model;
    private int _changes;

    private BufferUsageHint DrawType
    {
        get
        {
            if (_changes > MaxChanges)
                return BufferUsageHint.DynamicDraw;
            
            if (!SaveStatus)
                return BufferUsageHint.StreamDraw;

            if (Body?.GravityForce != Vector3.Zero)
                return BufferUsageHint.StaticDraw;

            return Body?.Force.Length > 0 ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;
        }
    }

    string IGameObject.SpaceName { get; set; }
    
    public UId Id { get; set; } = new();
    public IModel Model
    {
        get => _model;
        set
        {
            if (value is null || _model is not null && _model.Id == value.Id)
                return;
            
            _model = value;
            _changes += _changes > MaxChanges ? 0 : 1;
            ReloadBuffers();
        }
    }
    public IBody Body { get; set; }
    public ITransform Transform { get; private set; }
    public IVisual Visual { get; private set; }
    public bool SaveStatus { get; set; }

    protected GameObject()
    {
        Transform = new Transform();
        Visual = new Visual();
    }

    ~GameObject() => Dispose(false);

    public override string ToString()
        => $"object: {Id}\r\n" +
           string.Join(' ', Transform.Position) + "\r\n" +
           string.Join(' ', Transform.Scale) + "\r\n" +
           string.Join(' ', Transform.Rotation) + "\r\n" +
           string.Join(' ', Visual.Color) + "\r\n" +
           string.Join(' ', Visual.Visibility);
    
    public IGameObject Clone()
    {
        var obj = DeepClone();
        obj.Body = Body?.Clone();
        obj.Transform = Transform.Clone();
        obj.Model = Model;
        obj.Visual = Visual.Clone();
        obj.SaveStatus = SaveStatus;

        return obj;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public static IGameObject Find(string name)
    {
        var isFound = Game.GameObjects.TryGetValue(name, out var value);

        if (isFound)
            return value;

        throw new Exception("GameObject not found.");
    }

    public static IEnumerable<IGameObject> FindOrDefault(Predicate<IGameObject> predicate)
    {
        foreach (var (_, obj) in Game.GameObjects)
            if (predicate.Invoke(obj))
                yield return obj;
    }

    void IGameObject.Load()
    {
        // Получение точек позиции объекта в пространстве, текстуры в пространстве и цвета в виде массива float
        // и получение массива uint для Element object.
        var shader = Game.Shaders[Visual.Type];
        shader.Enable();
        
        // Создание нового Array object для каждого игрового объекта.
        _arrayObject = new ArrayObject();
        var drawType = DrawType;

        // Создание буферa для векторного представления.
        var arrayBuffer = new BufferObject(BufferTarget.ArrayBuffer);
        arrayBuffer.SetData(Model.Array.ToArray(), drawType);
        _arrayObject.AttachBuffer(arrayBuffer);
        
        // Получение индексов для трёх атрибутов (позиция, текстура и цвет).
        const int stride = Constants.Vector3Stride * 2 + Constants.Vector2Stride;
        var position = shader.GetAttributeLocation("aPosition");
        var normal = shader.GetAttributeLocation("inNormal");
        var texture = shader.GetAttributeLocation("inTexture");
        arrayBuffer.Enable();
        _arrayObject.EnableAttribute(position, Constants.Vector3Length, VertexAttribPointerType.Float, 
            stride, 0);
        _arrayObject.EnableAttribute(normal, Constants.Vector3Length, VertexAttribPointerType.Float, 
            stride, Constants.Vector3Stride);
        _arrayObject.EnableAttribute(texture, Constants.Vector2Length, VertexAttribPointerType.Float, 
            stride, Constants.Vector3Stride * 2);
        arrayBuffer.Disable();

        // Создание буфера для Element object.
        var elementBuffer = new BufferObject(BufferTarget.ElementArrayBuffer);
        elementBuffer.SetData(Model.Indices.ToArray(), drawType);
        _arrayObject.AttachBuffer(elementBuffer);
        _arrayObject.Disable();
        
        // Связывание ресурсов для текущего объекта.
        Visual.Texture?.Enable(); // А нахуя? А я уже не помню.
        shader.Disable();
    }

    void IGameObject.Draw(IEnumerable<IGameObject> lights)
    {
        Matrix4 GetMatrix()
        {
            if (Body is null)
                return Transform.ModelMatrix;

            var time = FrameTimeState.RenderTime;
            // SCALE
            var scale = Matrix4.Identity * Transform.ScaleMatrix;
            
            // ROTATION
            var angularAcceleration = Body.Torque * Body.InverseInertia;
            Body.AngularVelocity += angularAcceleration * time;
            Transform.Rotation += Body.AngularVelocity * time;
            var rotation = scale * Transform.RotationMatrix;

            // TRANSLATION
            var acceleration = Body.Force * Body.InverseMass + Body.GravityForce;
            Body.Velocity += acceleration * time;
            Transform.Position += Body.Velocity * time;
            
            return rotation * Transform.TranslationMatrix;
        }
        Matrix3 NormalMatrix(Matrix4 model)
        {
            var resultMatrix = new Matrix3(model.Row0.Xyz, model.Row1.Xyz, model.Row2.Xyz);
        
            return resultMatrix.Inverted();
        }
        
        var shader = Game.Shaders[Visual.Type];
        var light = lights.First();
        var modelMatrix = GetMatrix();
        
        shader.Enable();
        _arrayObject.Enable();
        Visual.Texture?.Enable();
        shader.SetMatrix4("model", modelMatrix);
        shader.SetMatrix4("view", Camera.View);
        shader.SetMatrix4("projection", Camera.Projection);
        shader.SetVector4("lightColor", Color4.ToXyz(light.Visual.Color));

        switch (Visual.Type)
        {
            case BodyVisualType.Color:
                var lightPos = modelMatrix * new Vector4(light.Transform.Position, 1);
                shader.SetMatrix3("normalMatrix", NormalMatrix(modelMatrix));
                shader.SetVector3("lightPosition", lightPos.Xyz);
                shader.SetVector4("inColor", Color4.ToXyz(Visual.Color));
                shader.SetVector3("viewPosition", Camera.Position);
                shader.SetLight("light", Visual.Light);
                break;
            case BodyVisualType.Texture:
                lightPos = modelMatrix * new Vector4(light.Transform.Position, 1);
                shader.SetMatrix3("normalMatrix", NormalMatrix(modelMatrix));
                shader.SetVector3("lightPosition", lightPos.Xyz);
                shader.SetVector3("viewPosition", Camera.Position);
                break;
            case BodyVisualType.Light:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Visual), "Visual error.");
        }
        
        _arrayObject.Draw(PrimitiveType.Triangles, 0, Model.Indices.Count, DrawElementsType.UnsignedInt);
        Visual.Texture?.Disable();
        shader.Disable();
    }
    
    void IGameResource.Remove()
    {
        var space = Game.Spaces[((IGameObject)this).SpaceName];
        space.GameObjects.Remove(this);
    }

    protected abstract GameObject DeepClone();

    private void ReloadBuffers()
    {
        if (_arrayObject is null)
            return;

        var shader = Game.Shaders[Visual.Type];
        shader.Enable();
        var drawType = DrawType;
        var arrayBuffer = _arrayObject.Buffers[BufferTarget.ArrayBuffer];
        arrayBuffer.SetData(Model.Array.ToArray(), drawType);
        var elementBuffer = _arrayObject.Buffers[BufferTarget.ElementArrayBuffer];
        elementBuffer.SetData(Model.Indices.ToArray(), drawType);
        shader.Disable();
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Body = null;
            Transform = null;
        }

        ReleaseUnmanagedResources();
        _disposed = true;
    }

    private void ReleaseUnmanagedResources() => _arrayObject.Dispose();
}
