using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

namespace TyzeEngine.Objects;

public abstract class GameObject : IGameObject, ICloneable, IDisposable
{
    private bool _disposed;

    public int Id { get; }
    public IModel Model { get; }
    public IReadOnlyList<IResource> Resources { get; private set; }
    public IPhysics Physics { get; }
    public List<ITrigger> Triggers { get; }
    public List<IScript> Scripts { get; }
    public virtual bool LoadError { get; private set; }

    protected GameObject(int id, IModel model, IEnumerable<string> resourcePaths, IPhysics physics)
    {
        Id = id;
        Model = model;
        Physics = physics;
        Triggers = new List<ITrigger>();
        Scripts = new List<IScript>();
        Resources = resourcePaths.Select(IResource.GetByPath).ToList();
    }
    
    ~GameObject() => Dispose(false);

    public virtual void Load()
    {
        foreach (var resource in Resources)
            ThreadPool.QueueUserWorkItem(_ => Load(resource));
    }

    public byte[] GetSaveData()
    {
        var id = BitConverter.GetBytes(Id);
        var model = Encoding.UTF8.GetBytes(Model.ToString());

        return id.Concat(ConstHelper.Separator).Concat(model).ToArray();
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

        Resources = null;
        _disposed = true;
    }

    private void Load(object obj)
    {
        var resource = (IResource)obj;
        resource.Load();

        if (resource.LoadError)
            LoadError = true;
    }
}
