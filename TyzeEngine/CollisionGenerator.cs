using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine;

public static class PhysicsGenerator
{
    public static CollisionHandler CollisionEnter = ResolveCollision2D;
    public static CollisionHandler CollisionStay;
    public static CollisionHandler CollisionExit;

    public static void Collision2D(IReadOnlyList<IGameObject> objects)
    {
        objects = objects.Where(obj => obj.Physics.IsEnabled).ToList();

        Parallel.ForEach(objects, thisObj =>
        {
            foreach (var otherObj in objects)
                if (!thisObj.Id.Equals(otherObj.Id))
                {
                    var args = thisObj.Physics.IsCollisionWith(thisObj, otherObj);
                    
                    if (args.IsCollides)
                        CollisionEnter.Invoke(args);
                }
            
            Force(thisObj, thisObj.Physics.GravityForce);
            Force(thisObj, thisObj.Physics.Velocity);
        });
    }

    public static void Force(IGameObject obj, Vector3 force) => obj.Translate(force.X, force.Y, force.Z);

    internal static CollisionEventArgs RectangleWithRectangle(IGameObject thisObj, IGameObject otherObj)
    {
        var args = new CollisionEventArgs(thisObj, otherObj);
        var thisPhysics = (RectanglePhysics)thisObj.Physics;
        var otherPhysics = (RectanglePhysics)otherObj.Physics;
        var n = otherObj.Position - thisObj.Position;
        var thisExtent = (thisPhysics.MaxX - thisPhysics.MinX) / 2;
        var otherExtend = (otherPhysics.MaxX - otherPhysics.MinX) / 2;
        var xOverlap = thisExtent + otherExtend - MathF.Abs(n.X);

        if (xOverlap <= 0) 
            return args;
        
        thisExtent = (thisPhysics.MaxY - thisPhysics.MinY) / 2;
        otherExtend = (otherPhysics.MaxY - otherPhysics.MinY) / 2;
        var yOverlap = thisExtent + otherExtend - MathF.Abs(n.Y);

        if (yOverlap <= 0) 
            return args;
        
        args.IsCollides = true;
            
        if (xOverlap > yOverlap)
        {
            args.Normal = n.X < 0 ? new Vector3(-1, 0, 0) : new Vector3(0, 0, 0);
            args.Penetration = xOverlap;
            
            return args;
        }

        args.Normal = n.Y < 0 ? new Vector3(0, -1, 0) : new Vector3(0, 1, 0);
        args.Penetration = yOverlap;

        return args;
    }
    
    internal static CollisionEventArgs CircleWithCircle(IGameObject thisObj, IGameObject otherObj)
    {
        var thisPhysics = (CirclePhysics)thisObj.Physics;
        var otherPhysics = (CirclePhysics)otherObj.Physics;

        return CirclesCollision(thisObj, otherObj, thisPhysics.Radius, otherPhysics.Radius);
    }

    internal static CollisionEventArgs RectangleWithCircle(IGameObject thisObj, IGameObject otherObj)
    {
        return null;
    }

    private static void ResolveCollision2D(CollisionEventArgs args)
    {
        PositionCorrection(args);
        var (thisObj, otherObj) = (args.ThisObject, args.OtherObject);
        var difference = otherObj.Physics.Velocity - thisObj.Physics.Velocity;
        var velocityAlongNormal = ScalarProduct2D(difference, args.Normal);

        if (velocityAlongNormal > 0)
            return;

        var e = MathF.Min(thisObj.Physics.Restitution, otherObj.Physics.Restitution);
        var j = (-(1 + e) * velocityAlongNormal) / (thisObj.Physics.InverseMass + otherObj.Physics.InverseMass);
        var impulse = VectorToScalarProduct(args.Normal, j);
        
        thisObj.Physics.Impulse(-thisObj.Physics.InverseMass * impulse);
        otherObj.Physics.Impulse(otherObj.Physics.InverseMass * impulse);
    }

    private static void PositionCorrection(CollisionEventArgs args)
    {
        const float percent = .2f;
        const float slop = .01f;
        var (obj1, obj2) = (args.ThisObject, args.OtherObject);
        var correction = MathF.Max(args.Penetration - slop, .0f) / (obj1.Physics.InverseMass 
            + obj2.Physics.InverseMass) * percent * args.Normal;
        var correction1 = -obj1.Physics.InverseMass * correction;
        var correction2 = obj2.Physics.InverseMass * correction;
        obj1.Translate(correction1.X, correction1.Y, correction1.Z);
        obj2.Translate(correction2.X, correction2.Y, correction2.Z);
    }

    private static float ScalarProduct2D(Vector3 vector1, Vector3 vector2) 
        => vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;

    private static Vector3 VectorToScalarProduct(Vector3 vector, float number) 
        => new(vector.X * number, vector.Y * number, vector.Z * number);

    private static CollisionEventArgs CirclesCollision(IGameObject thisObj, IGameObject otherObj, float thisRadius,
        float otherRadius)
    {
        var args = new CollisionEventArgs(thisObj, otherObj);
        var n = otherObj.Position - thisObj.Position;
        var r = thisRadius + otherRadius;
        r *= r;
        var length = n.X * n.X + n.Y * n.Y;

        if (length > r)
            return args;

        args.IsCollides = true;
        length = MathF.Sqrt(length);

        if (length != 0)
        {
            args.Penetration = r - length;
            args.Normal = Vector3.One;

            return args;
        }

        args.Penetration = thisRadius;
        args.Normal = new Vector3(1, 0, 0);

        return args;
    }
}