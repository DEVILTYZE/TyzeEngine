using System;
using System.Collections.Generic;
using System.Linq;
using TyzeEngine.Interfaces;

namespace TyzeEngine.GameStructure;

public sealed class Space : ISpace
{
    private bool _disposed;

    string ISpace.SceneOrSpaceName { get; set; }
    
    public UId Id { get; set; } = new();
    public List<ISpace> NeighbourSpaces { get; set; } = new();
    public List<IGameObject> GameObjects { get; set; } = new();
    public bool Loaded { get; set; }

    ~Space() => ReleaseUnmanagedResources();

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
    
    IEnumerable<UId> ISpace.GetResourceIds() 
        => from texture in GameObjects.Select(obj => obj.Visual.Texture) 
            where texture is not null 
            select texture.Id;
    
    void IGameResource.Remove()
    {
        var key = ((ISpace)this).SceneOrSpaceName;

        if (Game.Scenes.ContainsKey(key))
            Game.Scenes[key].CurrentPlace = null;
        else
            Game.Spaces[key].NeighbourSpaces.Remove(this);
    }

    public static ISpace Find(string name)
    {
        var isFound = Game.Spaces.TryGetValue(name, out var value);

        if (isFound)
            return value;

        throw new Exception("Place not found.");
    }

    private void ReleaseUnmanagedResources()
    {
        if (_disposed)
            return;

        NeighbourSpaces = null;

        foreach (var obj in GameObjects)
            obj?.Dispose();

        _disposed = true;
    }
}