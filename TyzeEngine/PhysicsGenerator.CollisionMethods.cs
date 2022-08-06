using System;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine;

public static partial class PhysicsGenerator
{
    // RECTANGLE WITH RECTANGLE
    
    internal static CollisionEventArgs RectangleWithRectangle(IBody bodyA, IBody bodyB)
    {
        var args = new CollisionEventArgs(bodyA, bodyB);
        var rectangleBodyA = (RectangleBody)bodyA;
        var rectangleBodyB = (RectangleBody)bodyB;

        // var penetrationA = FindAxisLeastPenetration(out var faceIndexA, rectangleBodyA, rectangleBodyB);
        //
        // if (penetrationA >= 0)
        //     return args;
        //
        // var penetrationB = FindAxisLeastPenetration(out var faceIndexB, rectangleBodyA, rectangleBodyB);
        //
        // if (penetrationB >= 0)
        //     return args;
        //
        // var referenceIndex = faceIndexA;
        // var flip = false;
        //
        // if (!BiasGreaterThan(penetrationA, penetrationB))
        // {
        //     (bodyA, bodyB) = (bodyB, bodyA);
        //     referenceIndex = faceIndexB;
        //     flip = true;
        // }
        
        // TODO: !!!

        var n = bodyB.Position - bodyA.Position;
        var extentA = (rectangleBodyA.MaxX - rectangleBodyA.MinX) / 2;
        var extentB = (rectangleBodyB.MaxX - rectangleBodyB.MinX) / 2;
        var xOverlap = extentA + extentB - MathF.Abs(n.X);

        if (xOverlap <= 0) 
            return args;
        
        extentA = (rectangleBodyA.MaxY - rectangleBodyA.MinY) / 2;
        extentB = (rectangleBodyB.MaxY - rectangleBodyB.MinY) / 2;
        var yOverlap = extentA + extentB - MathF.Abs(n.Y);

        if (yOverlap <= 0) 
            return args;
        
        args.IsCollides = true;
            
        if (xOverlap < yOverlap)
        {
            args.Normal = n.X < 0 ? new Vector3(-1, 0, 0) : new Vector3(1, 0, 0);
            args.Penetration = xOverlap;
            
            return args;
        }

        args.Normal = n.Y < 0 ? new Vector3(0, -1, 0) : new Vector3(0, 1, 0);
        args.Penetration = yOverlap;

        return args;
    }

    private static float FindAxisLeastPenetration(out int faceIndex, RectangleBody bodyA, RectangleBody bodyB)
    {
        var bestDistance = float.MinValue;
        var bestIndex = 0;
        var locker = new object();

        Parallel.For(0, bodyA.Vertices.Length, i =>
        {
            var normal = bodyA.Normals[i];
            var supportPoint = bodyB.GetSupportPoint(normal);
            var distance = Vector3.Dot(normal, supportPoint - bodyA.Vertices[i]);
            
            lock (locker)
            {
                if (distance <= bestDistance) 
                    return;
                
                bestDistance = distance;
                bestIndex = i;
            }
        });

        faceIndex = bestIndex;

        return bestDistance;
    }
    
    // private static void FindIncidentFace(out Vector3[] vectors, )
    // TODO: !!!
    
    // CIRCLE WITH CIRCLE
    
    internal static CollisionEventArgs CircleWithCircle(IBody bodyA, IBody bodyB)
    {
        var args = new CollisionEventArgs(bodyA, bodyB);
        var circleBodyA = (CircleBody)bodyA;
        var circleBodyB = (CircleBody)bodyB;
        var normal = circleBodyA.Position - circleBodyA.Position;
        var radius = circleBodyA.Radius + circleBodyB.Radius;
        radius *= radius;
        var distance = normal.LengthSquared;

        if (distance > radius)
            return args;

        args.IsCollides = true;
        args.ContactsCount = 1;
        distance = MathF.Sqrt(distance);

        if (distance != 0)
        {
            args.Penetration = radius - distance;
            args.Normal = Vector3.One;
            args.Contacts[0] = args.Normal * circleBodyA.Radius + circleBodyA.Position;

            return args;
        }

        args.Penetration = circleBodyA.Radius;
        args.Normal = new Vector3(1, 0, 0);
        args.Contacts[0] = circleBodyA.Position;

        return args;
    }

    // RECTANGLE WITH CIRCLE

    internal static CollisionEventArgs CircleWithRectangle(IBody circle, IBody rectangle)
    {
        var args = new CollisionEventArgs(circle, rectangle);
        var circleBody = (CircleBody)circle;
        var rectangleBody = (RectangleBody)rectangle;
        var center = circleBody.Position - rectangleBody.Position;

        var separation = float.MinValue;
        var faceNormal = 0;

        for (var i = 0; i < rectangleBody.Vertices.Length; ++i)
        {
            var s = Vector3.Dot(rectangleBody.Normals[i], center - rectangleBody.Vertices[i]);

            if (s > circleBody.Radius)
                return args;

            if (!(s > separation)) 
                continue;
            
            separation = s;
            faceNormal = i;
        }

        args.IsCollides = true;
        var faceVertex = rectangleBody.Vertices[faceNormal];
        var faceIndex = faceNormal + 1 < rectangleBody.Vertices.Length ? faceNormal + 1 : 0;
        var faceVertex2 = rectangleBody.Vertices[faceIndex];

        if (separation < float.Epsilon)
        {
            args.Normal = -rectangleBody.Normals[faceNormal];
            args.ContactsCount = 1;
            args.Contacts[0] = args.Normal * circleBody.Radius + circleBody.Position;
            args.Penetration = circleBody.Radius;

            return args;
        }

        var dot1 = Vector3.Dot(center - faceVertex, faceVertex2 - faceVertex);
        var dot2 = Vector3.Dot(center - faceVertex2, faceVertex - faceVertex2);
        args.Penetration = circleBody.Radius - separation;

        if (dot1 <= 0)
        {
            if (Vector3.DistanceSquared(center, faceVertex) > circleBody.Radius * circleBody.Radius)
                return args;

            args.Normal = Vector3.Normalize(faceVertex - center);
            args.ContactsCount = 1;
            args.Contacts[0] = faceVertex + rectangleBody.Position;
        }
        else if (dot2 <= 0)
        {
            if (Vector3.DistanceSquared(center, faceVertex2) > circleBody.Radius * circleBody.Radius)
                return args;

            args.Normal = Vector3.Normalize(faceVertex2 - center);
            args.ContactsCount = 1;
            args.Contacts[0] = faceVertex2 + rectangleBody.Position;
        }
        else
        {
            var normal = rectangleBody.Normals[faceNormal];

            if (Vector3.Dot(center - faceVertex, normal) > circleBody.Radius)
                return args;

            args.Normal = normal;
            args.ContactsCount = 1;
            args.Contacts[0] = args.Normal * circleBody.Radius + circleBody.Position;
        }

        return args;
    }
    
    internal static CollisionEventArgs RectangleWithCircle(IBody rectangle, IBody circle)
    {
        var args = CircleWithRectangle(circle, rectangle);
        args.Normal = -args.Normal;

        return args;
    }

    private static bool BiasGreaterThan(float numberA, float numberB)
    {
        const float kRelative = .95f;
        const float kAbsolute = .01f;

        return kRelative * numberA > numberB + kAbsolute;
    }
}