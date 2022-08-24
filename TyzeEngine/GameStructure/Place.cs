using System;
using System.Collections.Generic;
using System.Linq;
using TyzeEngine.Interfaces;

namespace TyzeEngine.GameStructure;

public sealed class Place : IPlace
{
    private bool _disposed;

    public UId Id { get; set; } = new();
    public List<IPlace> NeighbourPlaces { get; set; } = new();
    public List<IGameObject> GameObjects { get; set; } = new();
    public bool Loaded { get; set; }

    ~Place() => ReleaseUnmanagedResources();

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public static IPlace Find(string name)
    {
        var isFound = Game.Places.TryGetValue(name, out var value);

        return isFound ? value : null;
    }

    IEnumerable<UId> IPlace.GetResourceIds() 
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