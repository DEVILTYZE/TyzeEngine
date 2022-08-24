using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
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
                return BufferUsageHint.StaticDraw;

            if (Body is null) 
                return BufferUsageHint.StaticDraw;
            
            if (Body.Visibility is VisibilityType.Collapsed or VisibilityType.Hidden)
                return BufferUsageHint.StaticDraw;

            if (Body.Force.Length > 0)
                return BufferUsageHint.DynamicDraw;
                
            return Scripts.Count switch
            {
                > 0 when Triggers.Count > 0 => BufferUsageHint.StreamDraw,
                > 0 when Triggers.Count == 0 => BufferUsageHint.DynamicDraw,
                0 when Triggers.Count > 0 => BufferUsageHint.DynamicDraw,
                _ => BufferUsageHint.StaticDraw
            };
        }
    }

    public UId Id { get; set; } = new();
    public IModel Model { get; private set; }
    public IBody Body { get; set; }
    public IResource Texture { get; set; }
    public List<ITrigger> Triggers { get; private set; } = new();
    public List<IScript> Scripts { get; private set; } = new();
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
        obj.Scripts = Scripts;
        obj.Triggers = Triggers;
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

    public static IGameObject Find(string name)
    {
        var isFound = Game.GameObjects.TryGetValue(name, out var value);

        return isFound ? value : null;
    }

    protected abstract GameObject DeepClone();

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Body = null;
            Triggers = null;
            Scripts = null;
        }

        ReleaseUnmanagedResources();
        
        Texture = null;
        _disposed = true;
    }

    private void ReleaseUnmanagedResources() => ((IGameObject)this).ArrayObject.Dispose();
}
