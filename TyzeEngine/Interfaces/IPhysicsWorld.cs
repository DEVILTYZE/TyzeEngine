using System.Collections.Generic;

namespace TyzeEngine.Interfaces;

public interface IPhysicsWorld
{
    public void Step(IEnumerable<IGameObject> objects);
}