using TyzeEngine.Physics;

namespace TyzeEngine.Interfaces;

public interface ICollisionMethod
{
    CollisionPoints Test(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB);
}