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
    private static Queue<IModel> _modelQueue = new(DefaultCount);

    public static bool HasNewResources => _resourceQueue.Count > 0;
    public static bool HasNewModels => _modelQueue.Count > 0;

    public static void Add([NotNull] IResource resource) => _resourceQueue.Enqueue(resource);
    
    public static void AddRange([NotNull] IEnumerable<IResource> resources)
        => resources.ToList().ForEach(resource => _resourceQueue.Enqueue(resource));

    public static void Add([NotNull] IModel model) => _modelQueue.Enqueue(model);

    public static void AddRange([NotNull] IEnumerable<IModel> models)
        => models.Where(model => model.Loaded).ToList().ForEach(model => _modelQueue.Enqueue(model));

    public static IResource TakeResource() => _resourceQueue.Dequeue();

    public static IModel TakeModel() => _modelQueue.Dequeue();

    public static void Clear()
    {
        _resourceQueue.ToList().ForEach(resource => resource?.Dispose());
        _modelQueue.ToList().ForEach(model => model?.Dispose());

        _resourceQueue = new Queue<IResource>();
        _modelQueue = new Queue<IModel>();
    }
}