using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;
using TyzeEngine.Resources;

namespace TyzeEngine.Objects;

public abstract class GameObject : IGameObject, IDisposable
{
    private bool _disposed;
    private IBody _body;

    ArrayObject IGameObject.ArrayObject { get; set; }

    BufferUsageHint IGameObject.DrawType
    {
        get
        {
            if (!SaveStatus)
                return BufferUsageHint.StaticDraw;
            
            if (Body.Visibility is VisibilityType.Collapsed or VisibilityType.Hidden)
                return BufferUsageHint.StaticDraw;

            if (Body.IsEnabled && Body.Force.Length > 0)
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

    public Uid Id { get; }
    public Uid ModelId { get; }
    public List<Uid> ResourceIds { get; private set; }

    public IBody Body
    {
        get => _body;
        set
        {
            value.GameObject = this;
            _body = value.Clone();
        }
    }
    
    public List<ITrigger> Triggers { get; }
    public List<IScript> Scripts { get; }
    public bool SaveStatus { get; }

    protected GameObject(Uid modelId, IReadOnlyList<Uid> resourceIds = null, IBody body = null, bool notSave = false)
    {
        Id = new Uid();
        ModelId = modelId;
        Body = body is null ? new RectangleBody() : body.Clone();
        Body.GameObject = this;
        SaveStatus = !notSave;
        ResourceIds = resourceIds is null ? new List<Uid>() : resourceIds.ToList();
        Triggers = new List<ITrigger>();
        Scripts = new List<IScript>();
        
        if (resourceIds is not null) 
            ResourceIds = resourceIds.ToList();
    }

    protected GameObject(SaveGameObjectData saveData, IBody body, bool notSave = false) 
        : this(saveData.ModelId, saveData.ResourceIds, body, notSave)
    {
        Id = saveData.Id;
        Body = Activator.CreateInstance(Constants.AssemblyName, saveData.BodyName) as IBody;
        
        if (Body is not null)
        {
            Body.SetPosition(saveData.Position.X, saveData.Position.Y, saveData.Position.Z);
            Body.SetScale(saveData.Size.X, saveData.Size.Y, saveData.Size.Z);
            Body.SetRotation(saveData.Rotation.X, saveData.Rotation.Y, saveData.Rotation.Z);
            Body.SetColor(saveData.Color);
        }
        
        foreach (var trigger in Triggers.Where(trigger => saveData.TriggerDictionary.ContainsKey(trigger.Id)))
            trigger.IsTriggered = saveData.TriggerDictionary[trigger.Id];
    }
    
    ~GameObject() => Dispose(false);

    public void EnableResources(Dictionary<Uid, IResource> resources)
    {
        foreach (var id in ResourceIds)
            resources[id].Enable();
    }

    public void DisableResources(Dictionary<Uid, IResource> resources)
    {
        foreach (var id in ResourceIds)
            resources[id].Disable();
    }
    
    public override string ToString()
        => $"object: {Id.Value}\r\n" + 
           string.Join(' ', Body.Position) + "\r\n" + 
           string.Join(' ', Body.Size) + "\r\n" + 
           string.Join(' ', Body.Rotation) + "\r\n" +
           string.Join(' ', Body.Color) + "\r\n" +
           Body.GetType().Name +
           "\r\nend object;";

    public abstract IGameObject Clone();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            ((IGameObject)this).ArrayObject.Dispose();
        }

        ResourceIds = null;
        _disposed = true;
    }
}
