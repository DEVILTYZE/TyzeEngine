using System.Collections.Generic;

namespace TyzeEngine.Interfaces;

public interface IComplexObject
{
    List<IGameObject> Objects { get; }

    void Load();
}