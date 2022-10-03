using TyzeEngine.Interfaces;
using TyzeEngine.Physics.Bodies;

namespace TyzeEngine.Physics.Methods;

public class SphereToSphereMethod : ICollisionMethod
{
    public CollisionPoints Test(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB)
    {
        var sphereBodyA = (SphereBody)bodyA;
        var sphereBodyB = (SphereBody)bodyB;
        
        return new CollisionPoints { IsCollides = true };
    }
}