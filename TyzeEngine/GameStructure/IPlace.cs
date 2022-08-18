using System;
using System.Collections.Generic;
using TyzeEngine.Interfaces;

namespace TyzeEngine.GameStructure;

public interface IPlace : IDisposable
{
    Uid Id { get; }
    IReadOnlyList<IPlace> NeighbourPlaces { get; set; }
    List<IGameObject> GameObjects { get; }
    bool Loaded { get; set; }

    internal IEnumerable<Uid> GetResourceIds();
}