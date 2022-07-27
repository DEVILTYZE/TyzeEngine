using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

namespace TyzeEngine.Objects;

public record struct SaveGameObjectData
{
    public int Id { get; }
    public Uid[] ResourceIds { get; }
    public IModel Model { get; }
    public Dictionary<int, bool> TriggerDictionary { get; }

    public SaveGameObjectData(Uid[] resourceIds, byte[] data)
    {
        ResourceIds = resourceIds;
        Id = BitConverter.ToInt32(data);
        var triggersCount = BitConverter.ToInt32(data, sizeof(int));
        TriggerDictionary = new Dictionary<int, bool>(triggersCount);
        
        for (var i = 0; i < triggersCount; ++i)
        {
            var intIndex = i * Constants.SizeOfTrigger + 2 * sizeof(int);
            var boolIndex = i * Constants.SizeOfTrigger + 3 * sizeof(int);
            TriggerDictionary.Add(BitConverter.ToInt32(data, intIndex), BitConverter.ToBoolean(data, boolIndex));
        }

        var modelDataIndex = 2 * sizeof(int) + triggersCount * Constants.SizeOfTrigger;
        Model = IModel.GetByString(Encoding.UTF8.GetString(data[modelDataIndex..]));
    }
}

public abstract class GameObject : IGameObject, ISaveable, ICloneable, IDisposable
{
    private bool _disposed;

    ArrayObject IGameObject.ArrayObject { get; set; }
    BufferUsageHint IGameObject.DrawType => GetBufferType();

    public int Id { get; }
    public IModel Model { get; }
    public IReadOnlyList<Uid> ResourceIds { get; private set; }
    public IPhysics Physics { get; }
    public List<ITrigger> Triggers { get; }
    public List<IScript> Scripts { get; }
    public virtual bool LoadError { get; private set; }
    public bool SaveStatus { get; }

    protected GameObject(int id, IModel model, IReadOnlyList<Uid> resourceIds = null, IPhysics physics = null, bool notSave = false)
    {
        Id = id;
        Model = model;
        Physics = physics;
        SaveStatus = !notSave;
        ResourceIds = resourceIds is null ? new List<Uid>() : resourceIds.ToList();
        Triggers = new List<ITrigger>();
        Scripts = new List<IScript>();
        
        if (resourceIds is not null) 
            ResourceIds = resourceIds.ToList();
    }

    protected GameObject(SaveGameObjectData saveData, IPhysics physics = null, bool notSave = false) 
        : this(saveData.Id, saveData.Model, saveData.ResourceIds, physics, notSave)
    {
        foreach (var trigger in Triggers.Where(trigger => saveData.TriggerDictionary.ContainsKey(trigger.Id)))
            trigger.IsTriggered = saveData.TriggerDictionary[trigger.Id];
    }
    
    ~GameObject() => Dispose(false);

    public void EnableResources(List<IResource> resources)
    {
        resources = resources.Where(resource => ResourceIds.Any(id => id.Equals(resource.Id))).ToList();
        
        foreach (var resource in resources)
            resource.Enable();
    }

    public void DisableResources(List<IResource> resources)
    {
        resources = resources.Where(resource => ResourceIds.Any(id => id.Equals(resource.Id))).ToList();
        
        foreach (var resource in resources)
            resource.Disable();
    }

    public byte[] GetSaveData()
    {
        var id = BitConverter.GetBytes(Id);
        var triggerList = Triggers.Where(trigger => ((ISaveable)trigger).SaveStatus).ToList();
        var triggerCount = BitConverter.GetBytes(triggerList.Count);
        var triggers = triggerList.SelectMany(trigger => ((ISaveable)trigger).GetSaveData()).ToArray();
        var model = Encoding.UTF8.GetBytes(Model.ToString());

        return id.Concat(triggerCount).Concat(triggers).Concat(model).ToArray();
    }

    object ICloneable.Clone() => Clone();

    protected abstract IGameObject Clone();

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
            // something...
        }

        ResourceIds = null;
        _disposed = true;
    }

    private void Load(object obj)
    {
        var resource = (IResource)obj;
        resource.Load();

        if (resource.LoadError)
            LoadError = true;
    }

    private BufferUsageHint GetBufferType()
    {
        if (!SaveStatus)
            return BufferUsageHint.StaticDraw;

        if (Model.Visibility is VisibilityType.Collapsed or VisibilityType.Hidden)
            return BufferUsageHint.StaticDraw;

        if (Scripts.Count > 0 && Triggers.Count > 0)
            return BufferUsageHint.StreamDraw;

        return BufferUsageHint.DynamicDraw;
    }
}
