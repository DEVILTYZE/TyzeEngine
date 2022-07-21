using System.Collections.Generic;
using TyzeEngine.Resources;

namespace TyzeEngine.Interfaces;

public interface IGameObject
{
    int Id { get; }
    IModel Model { get; }
    IReadOnlyList<IResource> Resources { get; }
    IPhysics Physics { get; }
    List<ITrigger> Triggers { get; }
    List<IScript> Scripts { get; }
    bool LoadError { get; }

    void Load();
    byte[] GetSaveData();
}