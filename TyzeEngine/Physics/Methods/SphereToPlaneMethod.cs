using TyzeEngine.Interfaces;
using TyzeEngine.Physics.Bodies;

namespace TyzeEngine.Physics.Methods;

public class SphereToPlaneMethod : ICollisionMethod
{
    public CollisionPoints Test(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB)
    {
        var sphereBody = (SphereBody)bodyA;
        var planeBody = (PlaneBody)bodyB;
        
        return new CollisionPoints { IsCollides = true };
    }
}