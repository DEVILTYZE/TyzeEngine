using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

namespace TyzeEngine;

public static class LoadQueue
{
    private const int DefaultCount = 16;
    
    private static Queue<IResource> _resourceQueue = new(DefaultCount);
    private static Queue<IGameObject> _objectQueue = new(DefaultCount);
    private static Queue<IModel> _modelQueue = new(DefaultCount);

    public static bool HasNewResources => _resourceQueue.Count > 0;
    public static bool HasNewModels => _modelQueue.Count > 0;

    public static void Add([NotNull] IResource resource) => _resourceQueue.Enqueue(resource);
    
    public static void AddRange([NotNull] IEnumerable<IResource> resources)
    {
        foreach(var resource in resources)
            _resourceQueue.Enqueue(resource);
    }

    public static void Add([NotNull] IGameObject obj) => _objectQueue.Enqueue(obj);

    public static void AddRange([NotNull] IEnumerable<IGameObject> objects)
    {
        foreach (var obj in objects)
            _objectQueue.Enqueue(obj);
    }

    public static void Add([NotNull] IModel model)
    {
        if (!model.Loaded)
            _modelQueue.Enqueue(model);
    }
    
    public static void AddRange([NotNull] IEnumerable<IModel> models)
    {
        foreach (var model in models.Where(localModel => !localModel.Loaded))
            _modelQueue.Enqueue(model);
    }

    public static IResource TakeResource() => _resourceQueue.Dequeue();

    public static IModel TakeModel() => _modelQueue.Dequeue();
    
    public static IEnumerable<IGameObject> TakeObjects()
    {
        var objects = _objectQueue.ToArray();
        _objectQueue = new Queue<IGameObject>(DefaultCount);

        return objects;
    }

    public static void Clear()
    {
        foreach (var resource in _resourceQueue)
            resource?.Dispose();

        foreach (var obj in _objectQueue)
            obj?.Dispose();

        foreach (var model in _modelQueue)
            model?.Dispose();

        _resourceQueue = new Queue<IResource>();
        _objectQueue = new Queue<IGameObject>();
        _modelQueue = new Queue<IModel>();
    }
}