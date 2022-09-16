﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            Game.Scenes[key].CurrentSpace = NeighbourSpaces.FirstOrDefault(space => space is not null);
        else
            Game.Spaces[key].NeighbourSpaces.Remove(this);
    }
    
    /// <summary>
    /// Ищет пространство по имени среди всех добавленных в игру пространств.
    /// </summary>
    /// <param name="name">Имя пространства.</param>
    /// <returns>Объект пространства, приведённый к типу ISpace.</returns>
    /// <exception cref="ArgumentNullException">Имя равно null.</exception>
    /// <exception cref="Exception">Пространство не найдено.</exception>
    public static ISpace Find([NotNull] string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name), "Name was null.");
        
        var isFound = Game.Spaces.TryGetValue(name, out var value);

        return isFound ? value : throw new Exception("Space not found.");
    }

    private void ReleaseUnmanagedResources()
    {
        if (_disposed)
            return;

        NeighbourSpaces = null;
        GameObjects.ForEach(obj => obj?.Dispose());
        _disposed = true;
    }
}