using System;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics.Bodies;

namespace TyzeEngine.Physics.Methods;

public class CircleToCircleMethod : ICollisionMethod
{
    private readonly Random _random = new();
    
    public CollisionPoints Test(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB)
    {
        var cirBodyA = (CircleBody)bodyA;
        var cirBodyB = (CircleBody)bodyB;
        var normal = transformA.Position - transformB.Position;
        var radius = cirBodyA.Radius + cirBodyB.Radius;
        radius *= radius;
        
        if (normal.LengthSquared > radius)
            return CollisionPoints.NonCollides;

        var distance = normal.LengthFast;
        var points = new CollisionPoints { IsCollides = true };
        
        if (distance != 0)
        {
            points.Penetration = radius - distance;
            points.Normal = normal / distance;

            return points;
        }

        points.Penetration = MathF.Min(cirBodyA.Radius, cirBodyB.Radius);
        points.Normal = new Vector3((float)_random.NextDouble(), (float)_random.NextDouble(), 0);

        return points;
    }
}