using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine;

public static class PhysicsGenerator
{
    private static object _collisionLocker = new();
    private static object _gravityLocker = new();
    
    public static CollisionHandler CollisionEnter { get; set; } = ResolveCollision2D;
    public static CollisionHandler CollisionStay { get; set; }
    public static CollisionHandler CollisionExit { get; set; }

    public static void Collision2D(IReadOnlyList<IGameObject> objects)
    {
        Parallel.For(0, objects.Count - 1, i =>
        {
            for (var j = i + 1; j < objects.Count; ++j)
            {
                var args = objects[i].Body.IsCollisionWith(objects[i], objects[j]);

                if (args.IsCollides)
                    CollisionEnter.Invoke(args);
            }
        });
    }

    public static void Gravity(IReadOnlyList<IGameObject> objects)
    {
        if (objects.Count < 3)
            return;
        
        Parallel.For(0, objects.Count - 1, i =>
        {
            for (var j = i + 1; j < objects.Count; ++j)
            {
                var distance = Vector3.DistanceSquared(objects[i].Position, objects[j].Position);
                var gravityForce = Constants.GravityConstant * objects[i].Body.Mass * objects[j].Body.Mass / distance;
                var thisDirection = objects[j].Position - objects[i].Position;
                var otherDirection = objects[i].Position - objects[j].Position;

                lock (_gravityLocker)
                {
                    objects[i].Body.AddVelocity(gravityForce * objects[i].Body.GravityScale * thisDirection);
                    objects[j].Body.AddVelocity(gravityForce * objects[j].Body.GravityScale * otherDirection);
                }
            }
        });
    }
    
    internal static CollisionEventArgs RectangleWithRectangle(IGameObject thisObj, IGameObject otherObj)
    {
        var args = new CollisionEventArgs(thisObj, otherObj);
        var thisPhysics = (RectanglePhysics)thisObj.Body;
        var otherPhysics = (RectanglePhysics)otherObj.Body;

        if (thisPhysics.MaxX < otherPhysics.MinX || thisPhysics.MinX > otherPhysics.MaxX)
            return args;

        if (thisPhysics.MaxY < otherPhysics.MinY || thisPhysics.MinY > otherPhysics.MaxY)
            return args;
        
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
    
    internal static CollisionEventArgs CircleWithCircle(IGameObject thisObj, IGameObject otherObj)
    {
        var thisPhysics = (CirclePhysics)thisObj.Body;
        var otherPhysics = (CirclePhysics)otherObj.Body;

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
        var thisVelocity = thisObj.Body.Velocity + thisObj.Body.Force;
        var otherVelocity = otherObj.Body.Velocity + otherObj.Body.Force;
        var difference = otherVelocity - thisVelocity;
        var velocityAlongNormal = Vector3.Dot(difference, args.Normal);

        if (velocityAlongNormal > 0)
            return;

        var e = MathF.Min(thisObj.Body.Restitution, otherObj.Body.Restitution);
        var jn = -(1 + e) * velocityAlongNormal / (thisObj.Body.InverseMass + otherObj.Body.InverseMass);
        var impulse = args.Normal * jn;
        
        lock (_collisionLocker)
        {
            thisObj.Body.AddVelocity(-thisObj.Body.InverseMass * impulse);
            otherObj.Body.AddVelocity(otherObj.Body.InverseMass * impulse);
        }

        difference = otherObj.Body.Velocity - thisObj.Body.Velocity;
        var tangent = difference - Vector3.Dot(difference, args.Normal) * args.Normal;
        tangent.NormalizeFast();

        var jt = -Vector3.Dot(difference, tangent);
        jt /= thisObj.Body.InverseMass + otherObj.Body.InverseMass;

        var thisStaticFriction = jt * thisObj.Body.InverseMass * thisObj.Body.Force.LengthFast;
        var otherStaticFriction = jt * otherObj.Body.InverseMass * otherObj.Body.Force.LengthFast;
        var mu = (thisStaticFriction + otherStaticFriction) / 2;
        var frictionImpulse = MathF.Abs(jt) < jn * mu ? jt * tangent : -jn * tangent;
        
        lock (_collisionLocker)
        {
            thisObj.Body.AddVelocity(-thisObj.Body.InverseMass * frictionImpulse);
            otherObj.Body.AddVelocity(otherObj.Body.InverseMass * frictionImpulse);
        }
    }

    private static void PositionCorrection(CollisionEventArgs args)
    {
        const float percent = .2f;
        const float slop = .01f;
        var (obj1, obj2) = (args.ThisObject, args.OtherObject);
        var correction = MathF.Max(args.Penetration - slop, .0f) / (obj1.Body.InverseMass 
            + obj2.Body.InverseMass) * percent * args.Normal;
        var correction1 = -obj1.Body.InverseMass * correction;
        var correction2 = obj2.Body.InverseMass * correction;
        
        lock (_collisionLocker)
        {
            obj1.Translate(correction1.X, correction1.Y, correction1.Z);
            obj2.Translate(correction2.X, correction2.Y, correction2.Z);
        }
    }

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