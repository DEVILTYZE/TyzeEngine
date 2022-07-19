using System.Collections.Generic;

namespace TyzeEngine.Interfaces;

public interface IGroupObject
{
    List<IGameObject> Objects { get; }

    void Load();
}