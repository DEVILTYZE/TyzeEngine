using System;
using System.Collections.Generic;
using TyzeEngine.Interfaces;

namespace TyzeEngine.GameStructure;

public class Place : IPlace, IDisposable
{
    private bool _disposed;
    
    public int Id { get; }
    public IReadOnlyList<IPlace> NeighbourPlaces { get; set; }
    public IReadOnlyList<IGameObject> Objects { get; }
    public bool Loaded { get; set; }

    public Place(int id, IReadOnlyList<IPlace> neighbourPlaces, IReadOnlyList<IGameObject> objects)
    {
        Id = id;
        NeighbourPlaces = neighbourPlaces;
        Objects = objects;
        Loaded = false;
    }

    ~Place() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
        }
        
        NeighbourPlaces = null;

        foreach (var obj in Objects)
            (obj as IDisposable)?.Dispose();

        _disposed = true;
    }
}