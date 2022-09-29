using System.Collections.Generic;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public interface IPhysicsWorld
{
    public void Step(IEnumerable<IGameObject> objects);
}