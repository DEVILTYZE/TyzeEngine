using System;
using System.Collections.Generic;
using TyzeEngine.Interfaces;

namespace TyzeEngine.GameStructure;

public interface IPlace : IDisposable, IIdObject
{
    List<IPlace> NeighbourPlaces { get; set; }
    List<IGameObject> GameObjects { get; set; }
    bool Loaded { get; set; }

    internal IEnumerable<UId> GetResourceIds();
}