using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TyzeEngine.Interfaces;

namespace TyzeEngine;

internal static class LoadQueue
{
    private const int DefaultCount = 16;
    
    private static Queue<IModel> _modelQueue = new(DefaultCount);

    internal static bool HasNewModels => _modelQueue.Count > 0;

    internal static void Add([NotNull] IModel model) => _modelQueue.Enqueue(model);

    internal static void AddRange([NotNull] IEnumerable<IModel> models) => 
        models.Where(model => model.Loaded).ToList().ForEach(model => _modelQueue.Enqueue(model));

    internal static IModel TakeModel() => _modelQueue.Dequeue();

    internal static void Clear()
    {
        _modelQueue.ToList().ForEach(model => model?.Dispose());
        _modelQueue = new Queue<IModel>();
    }
}