using System.Collections.Generic;
using TyzeEngine.Interfaces;

namespace TyzeEngine.GameStructure;

public interface IPlace
{
    int Id { get; }
    IReadOnlyList<IPlace> NeighbourPlaces { get; set; }
    IReadOnlyList<IGameObject> Objects { get; }
    bool Loaded { get; set; }
}