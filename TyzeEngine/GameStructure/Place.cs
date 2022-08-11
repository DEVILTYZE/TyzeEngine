using System;
using System.Collections.Generic;
using System.Linq;
using TyzeEngine.Interfaces;

namespace TyzeEngine.GameStructure;

public sealed class Place : IPlace, IDisposable
{
    private bool _disposed;
    
    public Uid Id { get; }
    public IReadOnlyList<IPlace> NeighbourPlaces { get; set; }
    public List<IGameObject> GameObjects { get; }
    public bool Loaded { get; set; }

    public Place(IReadOnlyList<IPlace> neighbourPlaces, List<IGameObject> objects)
    {
        Id = new Uid();
        NeighbourPlaces = neighbourPlaces;
        GameObjects = objects;
        Loaded = false;
    }

    ~Place() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    IEnumerable<Uid> IPlace.GetResourceIds() => GameObjects.SelectMany(obj => obj.ResourceIds ?? new List<Uid>());

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
        }
        
        NeighbourPlaces = null;

        foreach (var obj in GameObjects)
            (obj as IDisposable)?.Dispose();

        _disposed = true;
    }
}