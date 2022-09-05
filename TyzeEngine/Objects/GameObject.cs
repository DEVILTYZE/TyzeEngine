using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class GameObject : IGameObject
{
    private bool _disposed;

    ArrayObject IGameObject.ArrayObject { get; set; }
    BufferUsageHint IGameObject.DrawType
    {
        get
        {
            if (!SaveStatus)
                return BufferUsageHint.StreamDraw;

            if (Body?.GravityForce != Vector3.Zero)
                return BufferUsageHint.StaticDraw;

            return Body?.Force.Length > 0 ? BufferUsageHint.DynamicDraw : BufferUsageHint.StaticDraw;
        }
    }

    public UId Id { get; set; } = new();
    public IModel Model { get; private set; }
    public IBody Body { get; set; }
    public ITransform Transform { get; private set; }
    public bool SaveStatus { get; set; }

    protected GameObject(IModel model)
    {
        Model = model;
        Transform = new Transform();
    }

    ~GameObject() => Dispose(false);

    public override string ToString()
        => $"object: {Id}\r\n" +
           string.Join(' ', Transform.Position) + "\r\n" +
           string.Join(' ', Transform.Scale) + "\r\n" +
           string.Join(' ', Transform.Rotation) + "\r\n" +
           string.Join(' ', Transform.Color);
    
    public IGameObject Clone()
    {
        var obj = DeepClone();
        obj.Body = Body?.Clone();
        obj.Transform = Transform.Clone();
        obj.Model = Model;
        obj.SaveStatus = SaveStatus;

        return obj;
    }

    void IGameObject.SetModel(IModel model) => Model = model;
    
    void IGameObject.Load(Shader shader)
    {
        // Создание нового Array object для каждого игрового объекта.
        ((IGameObject)this).ArrayObject = new ArrayObject();
        var arrayObject = ((IGameObject)this).ArrayObject;
        var drawType = ((IGameObject)this).DrawType;
        arrayObject.Enable();
        
        // Получение точек позиции объекта в пространстве, текстуры в пространстве и цвета в виде массива float
        // и получение массива uint для Element object.
        var vertices = Model.GetVectorArray(this);

        // Создание буферa для векторного представления.
        var buffer = new BufferObject(BufferTarget.ArrayBuffer);
        buffer.SetData(vertices.Item1, drawType);
        arrayObject.AttachBuffer(buffer, 0);
        
        // Получение индексов для трёх атрибутов (позиция, текстура и цвет).
        var position = shader.GetAttributeLocation("aPosition");
        var texture = shader.GetAttributeLocation("inTexture");
        var color = shader.GetAttributeLocation("inColor");
        
        // Настройка атрибутов.
        buffer.Enable();
        arrayObject.EnableAttribute(position, Constants.Vector3Length, VertexAttribPointerType.Float, 
            Constants.VectorStride, 0);
        arrayObject.EnableAttribute(texture, Constants.Vector2Length, VertexAttribPointerType.Float, 
            Constants.VectorStride, Constants.Vector3Stride);
        arrayObject.EnableAttribute(color, Constants.Vector4Length, VertexAttribPointerType.Float, 
            Constants.VectorStride, Constants.Vector3Stride + Constants.Vector2Stride);
        buffer.Disable();

        // Создание буфера для Element object.
        var indicesBuffer = new BufferObject(BufferTarget.ElementArrayBuffer);
        indicesBuffer.SetData(vertices.Item2, drawType);
        arrayObject.AttachBuffer(indicesBuffer, vertices.Item2.Length);
        arrayObject.Disable();
        
        // Связывание ресурсов для текущего объекта.
        Transform.Texture?.Enable(); // А нахуя? А я уже не помню.
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public static IGameObject FindOrDefault(string name)
    {
        var isFound = Game.GameObjects.TryGetValue(name, out var value);

        return isFound ? value : null;
    }

    public static IEnumerable<IGameObject> FindOrDefault(Predicate<IGameObject> predicate)
    {
        foreach (var (_, obj) in Game.GameObjects)
            if (predicate.Invoke(obj))
                yield return obj;
    }

    protected abstract GameObject DeepClone();

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

    private void ReleaseUnmanagedResources() => ((IGameObject)this).ArrayObject.Dispose();
}
