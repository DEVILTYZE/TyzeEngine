using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

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
    public IResource Texture { get; set; }
    public bool SaveStatus { get; set; }

    protected GameObject(IModel model) => Model = model;

    ~GameObject() => Dispose(false);

    public override string ToString()
        => $"object: {Id}\r\n" +
           string.Join(' ', Body.Position) + "\r\n" +
           string.Join(' ', Body.Scale) + "\r\n" +
           string.Join(' ', Body.Rotation) + "\r\n" +
           string.Join(' ', Body.Color) +
           "\r\nend object;";
    
    public IGameObject Clone()
    {
        var obj = DeepClone();
        obj.Body = Body?.Clone();
        obj.Model = Model;
        obj.Texture = Texture;
        obj.SaveStatus = SaveStatus;

        return obj;
    }

    void IGameObject.SetModel(IModel model) => Model = model;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public static IGameObject FindOrDefault(string name)
    {
        var isFound = Game.Instance.GameObjects.TryGetValue(name, out var value);

        return isFound ? value : null;
    }

    public static IEnumerable<IGameObject> FindOrDefault(Predicate<IGameObject> predicate)
    {
        foreach (var (_, obj) in Game.Instance.GameObjects)
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
            Texture = null;
        }

        ReleaseUnmanagedResources();
        _disposed = true;
    }

    private void ReleaseUnmanagedResources() => ((IGameObject)this).ArrayObject.Dispose();
}
