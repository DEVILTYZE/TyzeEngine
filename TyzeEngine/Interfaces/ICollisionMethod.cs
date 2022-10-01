using TyzeEngine.Physics;

namespace TyzeEngine.Interfaces;

public interface ICollisionMethod
{
    CollisionPoints Invoke(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB);
}