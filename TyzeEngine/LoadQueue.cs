using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TyzeEngine.Resources;

namespace TyzeEngine;

public static class LoadQueue
{
    private const int DefaultCount = 16;
    
    private static Queue<IModel> _modelQueue = new(DefaultCount);

    public static bool HasNewModels => _modelQueue.Count > 0;

    public static void Add([NotNull] IModel model) => _modelQueue.Enqueue(model);

    public static void AddRange([NotNull] IEnumerable<IModel> models)
        => models.Where(model => model.Loaded).ToList().ForEach(model => _modelQueue.Enqueue(model));

    public static IModel TakeModel() => _modelQueue.Dequeue();

    public static void Clear()
    {
        _modelQueue.ToList().ForEach(model => model?.Dispose());
        _modelQueue = new Queue<IModel>();
    }
}