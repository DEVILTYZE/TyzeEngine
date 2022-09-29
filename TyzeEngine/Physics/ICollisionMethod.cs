using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public interface ICollisionMethod
{
    CollisionPoints Invoke(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB);
}