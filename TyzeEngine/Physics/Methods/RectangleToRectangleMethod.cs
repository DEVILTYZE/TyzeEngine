using System;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics.Bodies;

namespace TyzeEngine.Physics.Methods;

public class RectangleToRectangleMethod : ICollisionMethod
{
    public CollisionPoints Test(IBody bodyA, ITransform transformA, IBody bodyB, ITransform transformB)
    {
        var rectBodyA = (RectangleBody)bodyA;
        var rectBodyB = (RectangleBody)bodyB;
        var normal = transformA.Position - transformB.Position;
        var aExtend = (rectBodyA.Max.X - rectBodyA.Min.X) / 2;
        var bExtend = (rectBodyB.Max.X - rectBodyB.Min.X) / 2;
        var xOverlap = aExtend + bExtend - MathF.Abs(normal.X);
        
        if (xOverlap <= 0)
            return CollisionPoints.NonCollides;
        
        aExtend = (rectBodyA.Max.Y - rectBodyA.Min.Y) / 2;
        bExtend = (rectBodyB.Max.Y - rectBodyB.Min.Y) / 2;
        var yOverlap = aExtend + bExtend - MathF.Abs(normal.Y);
        
        if (yOverlap <= 0)
            return CollisionPoints.NonCollides;

        var points = new CollisionPoints { IsCollides = true };
        
        if (xOverlap > yOverlap)
        {
            points.Normal = normal.X < 0 ? -Vector3.UnitX : Vector3.Zero;
            points.Penetration = xOverlap;

            return points;
        }

        points.Normal = normal.Y < 0 ? Vector3.UnitY : -Vector3.UnitY;
        points.Penetration = yOverlap;

        return points;
    }
}