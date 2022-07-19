using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class GameObject : IGameObject, ICloneable, IDisposable
{
    private bool _disposed;

    public int Id { get; }
    public IVectorArray VectorStructure { get; }
    public IVectorArray VisualStructure { get; }
    public IReadOnlyList<IResource> Resources { get; private set; }
    public IPhysics Physics { get; }
    public List<ITrigger> Triggers { get; }
    public List<IScript> Scripts { get; }
    public virtual bool LoadError { get; private set; }

    protected GameObject(int id, IVectorArray vectorStructure, IEnumerable<string> resourcePaths, IPhysics physics)
    {
        Id = id;
        VectorStructure = vectorStructure;
        VisualStructure = new VectorArray(new[] { 1.0f, .5f, .2f, 1.0f }, ArrayType.FourDimensions);
        Physics = physics;
        Triggers = new List<ITrigger>();
        Scripts = new List<IScript>();
        Resources = resourcePaths.Select(path => new Resource(path)).Cast<IResource>().ToList();
    }
    
    protected GameObject(int id, IVectorArray vectorStructure, IVectorArray visualStructure, 
        IEnumerable<string> resourcePaths, IPhysics physics) 
        : this(id, vectorStructure, resourcePaths, physics) => VisualStructure = visualStructure;
    
    ~GameObject() => Dispose(false);

    public virtual void Load()
    {
        foreach (var resource in Resources)
            ThreadPool.QueueUserWorkItem(_ => Load(resource));
    }

    public float[] GetVectorArray()
    {
        if (VectorStructure.Type == ArrayType.Unknown)
            throw new Exception("Vector structure is not created.");

        if (VisualStructure.Type == ArrayType.Unknown)
            return VectorStructure.GetArray();

        var vectorStructureArray = VectorStructure.GetArray();
        var visualStructureArray = VisualStructure.GetArray();
        var length = vectorStructureArray.Length + visualStructureArray.Length;
        var resultArray = new float[length];
        int countVector = 1, countVisual = 0;
        resultArray[0] = vectorStructureArray[0];

        for (var i = 0; i < length; ++i)
        {
            if (countVector % (int)VectorStructure.Type != 0)
            {
                resultArray[i] = vectorStructureArray[countVector];
                ++countVector;
            }
            else if (countVisual % (int)VisualStructure.Type != 0)
            {
                resultArray[i] = visualStructureArray[countVisual];
                ++countVisual;
            }
        }

        return resultArray;
    }

    object ICloneable.Clone() => Clone();

    protected abstract GameObject Clone();

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
