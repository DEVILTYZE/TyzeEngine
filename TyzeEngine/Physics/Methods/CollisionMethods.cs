using System;
using System.Collections.Generic;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics.Bodies;

namespace TyzeEngine.Physics.Methods;

public static class CollisionMethods
{
    private static readonly Dictionary<TypeKey, ICollisionMethod> MethodList = new()
    {
        { new TypeKey(typeof(PlaneBody),     typeof(PlaneBody)),     new PlaneToPlaneMethod()         },
        { new TypeKey(typeof(SphereBody),    typeof(SphereBody)),    new SphereToSphereMethod()       },
        { new TypeKey(typeof(SphereBody),    typeof(PlaneBody)),     new SphereToPlaneMethod()        },
        { new TypeKey(typeof(PlaneBody),     typeof(SphereBody)),    new PlaneToSphereMethod()        },
        { new TypeKey(typeof(MeshBody),      typeof(MeshBody)),      new MeshToMeshMethod()           },
        { new TypeKey(typeof(RectangleBody), typeof(RectangleBody)), new RectangleToRectangleMethod() },
        { new TypeKey(typeof(CircleBody),    typeof(CircleBody)),    new CircleToCircleMethod()       }
    };

    public static void Add(TypeKey key, ICollisionMethod method)
    {
        if (MethodList.ContainsKey(key))
            throw new ArgumentException("Method with this key already exists.");
        
        MethodList.Add(key, method);
    }

    public static bool Remove(TypeKey key) => MethodList.Remove(key);

    public static CollisionPoints Test(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB)
    {
        var key = new TypeKey(bodyA.GetType(), bodyB.GetType());
        return MethodList.ContainsKey(key) 
            ? MethodList[key].Test(bodyA, transformA, bodyB, transformB) 
            : CollisionPoints.NonCollides;
    }
}
