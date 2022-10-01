using System;
using System.Collections.Generic;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public static class CollisionMethods
{
    private static readonly Dictionary<TypeKey, ICollisionMethod> MethodList = new()
    {
        { new TypeKey(typeof(PlaneBody), typeof(PlaneBody)), new PlaneToPlaneMethod() },
        { new TypeKey(typeof(SphereBody), typeof(SphereBody)), new SphereToSphereMethod() },
        { new TypeKey(typeof(SphereBody), typeof(PlaneBody)), new SphereToPlaneMethod() },
        { new TypeKey(typeof(PlaneBody), typeof(SphereBody)), new PlaneToSphereMethod() },
        { new TypeKey(typeof(MeshBody), typeof(MeshBody)), new MeshToMeshMethod() }
    };

    public static void Add(TypeKey key, ICollisionMethod method)
    {
        if (MethodList.ContainsKey(key))
            throw new ArgumentException("Method with this key already exists.");
        
        MethodList.Add(key, method);
    }

    public static bool Remove(TypeKey key) => MethodList.Remove(key);

    public static CollisionPoints Invoke(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB)
    {
        var key = new TypeKey(bodyA.GetType(), bodyB.GetType());

        return MethodList.ContainsKey(key) 
            ? MethodList[key].Invoke(bodyA, transformA, bodyB, transformB) 
            : CollisionPoints.NonCollides;
    }
}

public class PlaneToPlaneMethod : ICollisionMethod
{
    public CollisionPoints Invoke(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB)
    {
        var planeBodyA = (PlaneBody)bodyA;
        var planeBodyB = (PlaneBody)bodyB;
        
        return new CollisionPoints { IsCollides = true };
    }
}

public class SphereToSphereMethod : ICollisionMethod
{
    public CollisionPoints Invoke(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB)
    {
        var sphereBodyA = (SphereBody)bodyA;
        var sphereBodyB = (SphereBody)bodyB;
        
        return new CollisionPoints { IsCollides = true };
    }
}

public class SphereToPlaneMethod : ICollisionMethod
{
    public CollisionPoints Invoke(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB)
    {
        var sphereBody = (SphereBody)bodyA;
        var planeBody = (PlaneBody)bodyB;
        
        return new CollisionPoints { IsCollides = true };
    }
}

public class PlaneToSphereMethod : ICollisionMethod
{
    public CollisionPoints Invoke(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB)
    {
        var points = new SphereToPlaneMethod().Invoke(bodyB, transformB, bodyA, transformA);
        (points.A, points.B) = (points.B, points.A);
        points.Normal = -points.Normal;
        
        return points;
    }
}

public class MeshToMeshMethod : ICollisionMethod
{
    public CollisionPoints Invoke(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB)
    {
        var meshBodyA = (MeshBody)bodyA;
        var meshBodyB = (MeshBody)bodyB;
        meshBodyA.Transform = transformA.ModelMatrix;
        meshBodyB.Transform = transformB.ModelMatrix;

        return GJK.TestCollision(meshBodyA, meshBodyB);
    }
}
