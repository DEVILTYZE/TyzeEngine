using System;
using System.Collections.Generic;
using System.Linq;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

namespace TyzeEngine;

public static class LoadQueue
{
    private static readonly Queue<IResource> ResourceQueue = new();
    private static readonly Queue<IGameObject> ObjectQueue = new();
    private static readonly Queue<IModel> ModelQueue = new();

    public static bool HasNewResources => ResourceQueue.Count > 0;
    public static bool HasNewModels => ModelQueue.Count > 0;

    public static void Add(IResource resource) => ResourceQueue.Enqueue(resource);
    
    public static void AddRange(IEnumerable<IResource> resources)
    {
        foreach(var resource in resources)
            ResourceQueue.Enqueue(resource);
    }

    public static void Add(IGameObject obj) => ObjectQueue.Enqueue(obj);

    public static void Add(IModel model)
    {
        if (!model.Loaded)
            ModelQueue.Enqueue(model);
    }
    
    public static void AddRange(IEnumerable<IModel> models)
    {
        foreach (var model in models.Where(localModel => !localModel.Loaded))
            ModelQueue.Enqueue(model);
    }

    public static IResource TakeLastResource() => ResourceQueue.Dequeue();

    public static IModel TakeLastModel() => ModelQueue.Dequeue();
    
    public static IEnumerable<IGameObject> TakeObjects()
    {
        var objects = ObjectQueue.ToArray();
        ObjectQueue.Clear();

        return objects;
    }

    public static void Clear()
    {
        foreach (var resource in ResourceQueue)
            ((IDisposable)resource).Dispose();

        foreach (var obj in ObjectQueue)
            ((IDisposable)obj).Dispose();
        
        ResourceQueue.Clear();
        ObjectQueue.Clear();
    }
}