using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics.Methods;

public class PlaneToPlaneMethod : ICollisionMethod
{
    public CollisionPoints Test(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB) =>
        CollisionPoints.NonCollides;
}