using System.Collections.Generic;

namespace TyzeEngine.Interfaces;

public interface IGameObject
{
    int Id { get; }
    IVectorArray VectorStructure { get; }
    IVectorArray VisualStructure { get; }
    IReadOnlyList<IResource> Resources { get; }
    IPhysics Physics { get; }
    List<ITrigger> Triggers { get; }
    List<IScript> Scripts { get; }
    bool LoadError { get; }

    void Load();
    float[] GetVectorArray();
}