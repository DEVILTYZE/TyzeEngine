using System;
using System.Collections.Generic;
using System.Linq;
using TyzeEngine.Interfaces;

namespace TyzeEngine.GameStructure;

public sealed class Place : IPlace
{
    private bool _disposed;

    public Uid Id { get; } = new();
    public IReadOnlyList<IPlace> NeighbourPlaces { get; set; }
    public List<IGameObject> GameObjects { get; }
    public bool Loaded { get; set; }

    public Place(IReadOnlyList<IPlace> neighbourPlaces, List<IGameObject> objects)
    {
        NeighbourPlaces = neighbourPlaces;
        GameObjects = objects;
    }

    ~Place() => ReleaseUnmanagedResources();

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    IEnumerable<Uid> IPlace.GetResourceIds() 
        => from texture in GameObjects.Select(obj => obj.Texture) 
            where texture is not null 
            select texture.Id;

    private void ReleaseUnmanagedResources()
    {
        if (_disposed)
            return;

        NeighbourPlaces = null;

        foreach (var obj in GameObjects)
            obj?.Dispose();

        _disposed = true;
    }
}