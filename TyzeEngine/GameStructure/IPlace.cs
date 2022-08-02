using System.Collections.Generic;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

namespace TyzeEngine.GameStructure;

public interface IPlace
{
    int Id { get; }
    IReadOnlyList<IPlace> NeighbourPlaces { get; set; }
    List<IGameObject> GameObjects { get; }
    bool Loaded { get; set; }

    internal IEnumerable<Uid> GetResourceIds();
}