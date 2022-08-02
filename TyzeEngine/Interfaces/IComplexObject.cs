using System.Collections.Generic;

namespace TyzeEngine.Interfaces;

public interface IComplexObject
{
    List<IGameObject> GameObjects { get; }
}