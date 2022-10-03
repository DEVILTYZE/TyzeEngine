using TyzeEngine.Interfaces;
using TyzeEngine.Physics.Bodies;

namespace TyzeEngine.Physics.Methods;

public class MeshToMeshMethod : ICollisionMethod
{
    public CollisionPoints Test(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB)
    {
        var meshBodyA = (MeshBody)bodyA;
        var meshBodyB = (MeshBody)bodyB;
        meshBodyA.Transform = transformA.ModelMatrix;
        meshBodyB.Transform = transformB.ModelMatrix;

        return GJK.TestCollision(meshBodyA, meshBodyB);
    }
}