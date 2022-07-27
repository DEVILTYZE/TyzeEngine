using System.Collections.Generic;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

namespace TyzeEngine;

public static class LoadQueue
{
    private static readonly Queue<IResource> ResourceQueue = new();
    private static readonly Queue<IGameObject> ObjectQueue = new();

    public static bool HasNewResources => ResourceQueue.Count > 0;

    public static void Add(IResource resource) => ResourceQueue.Enqueue(resource);
    
    public static void AddRange(IEnumerable<IResource> resources)
    {
        foreach(var resource in resources)
            ResourceQueue.Enqueue(resource);
    }

    public static void Add(IGameObject obj) => ObjectQueue.Enqueue(obj);

    public static IResource TakeLast() => ResourceQueue.Dequeue();
    
    public static IEnumerable<IGameObject> TakeObjects()
    {
        var objects = ObjectQueue.ToArray();
        ObjectQueue.Clear();

        return objects;
    }

    public static void Clear()
    {
        ResourceQueue.Clear();
        ObjectQueue.Clear();
    }
}