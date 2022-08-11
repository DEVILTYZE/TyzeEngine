using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class GameObject : IGameObject, IDisposable
{
    private bool _disposed;
    private int _currentResourceIndex;

    ArrayObject IGameObject.ArrayObject { get; set; }
    BufferUsageHint IGameObject.DrawType
    {
        get
        {
            if (!SaveStatus)
                return BufferUsageHint.StaticDraw;

            if (Body is not null)
            {
                if (Body.Visibility is VisibilityType.Collapsed or VisibilityType.Hidden)
                    return BufferUsageHint.StaticDraw;

                if (Body.Force.Length > 0)
                    return BufferUsageHint.DynamicDraw;
            }

            return Scripts.Count switch
            {
                > 0 when Triggers.Count > 0 => BufferUsageHint.StreamDraw,
                > 0 when Triggers.Count == 0 => BufferUsageHint.DynamicDraw,
                0 when Triggers.Count > 0 => BufferUsageHint.DynamicDraw,
                _ => BufferUsageHint.StaticDraw
            };;
        }
    }

    public Uid Id { get; }
    public IModel Model { get; private set; }

    public IBody Body { get; set; }
    public List<Uid> ResourceIds { get; set; }
    public Uid ResourceId => ResourceIds.Count > 0 ? ResourceIds[_currentResourceIndex] : Uid.Zero;
    public List<ITrigger> Triggers { get; private set; }
    public List<IScript> Scripts { get; private set; }
    public bool SaveStatus { get; set; }

    protected GameObject(IModel model)
    {
        _currentResourceIndex = 0;
        Id = new Uid();
        Model = model;
        ResourceIds = new List<Uid>();
        Triggers = new List<ITrigger>();
        Scripts = new List<IScript>();
    }

    protected GameObject(SaveGameObjectData saveData, bool notSave = false) : this(saveData.Model)
    {
        Id = saveData.Id;
        
        if (string.CompareOrdinal(saveData.BodyName, "null") != 0)
        {
            Body.Position = saveData.Position;
            Body.Scale = saveData.Size;
            Body.Rotation = saveData.Rotation;
            Body.Color = saveData.Color;
        }
        
        foreach (var trigger in Triggers.Where(trigger => saveData.TriggerDictionary.ContainsKey(trigger.Id)))
            trigger.IsTriggered = saveData.TriggerDictionary[trigger.Id];
    }
    
    ~GameObject() => Dispose(false);

    public void NextResource() 
        => _currentResourceIndex = _currentResourceIndex < ResourceIds.Count - 1 ? _currentResourceIndex + 1 : 0;

    public void PreviousResource()
        => _currentResourceIndex = _currentResourceIndex >= 0 ? _currentResourceIndex - 1 : ResourceIds.Count - 1;

    public override string ToString()
        => $"object: {Id.Value}\r\n" +
           string.Join(' ', Body.Position) + "\r\n" +
           string.Join(' ', Body.Scale) + "\r\n" +
           string.Join(' ', Body.Rotation) + "\r\n" +
           string.Join(' ', Body.Color) +
           "\r\nend object;";


    public IGameObject Clone()
    {
        var obj = CloneObject();
        obj.Body = Body.Clone();
        obj.Model = Model;
        obj.Scripts = Scripts;
        obj.Triggers = Triggers;
        obj.ResourceIds = ResourceIds;
        obj.SaveStatus = SaveStatus;

        return obj;
    }

    protected abstract GameObject CloneObject();

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
            ((IGameObject)this).ArrayObject.Dispose();

        ResourceIds = null;
        _disposed = true;
    }
}
