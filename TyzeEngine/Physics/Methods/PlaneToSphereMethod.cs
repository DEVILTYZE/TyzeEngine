using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics.Methods;

public class PlaneToSphereMethod : ICollisionMethod
{
    public CollisionPoints Test(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB)
    {
        var points = new SphereToPlaneMethod().Test(bodyB, transformB, bodyA, transformA);
        (points.A, points.B) = (points.B, points.A);
        points.Normal = -points.Normal;
        
        return points;
    }
}