using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine;

public static class PhysicsGenerator
{
    private static readonly object CollisionLocker = new();
    
    public static CollisionHandler CollisionEnter { get; set; } = ResolveCollision;
    public static CollisionHandler CollisionStay { get; set; }
    public static CollisionHandler CollisionExit { get; set; }

    public static void DetermineCollision(IReadOnlyList<IGameObject> objects)
    {
        Parallel.For(0, objects.Count - 1, i =>
        {
            for (var j = i + 1; j < objects.Count; ++j)
            {
                var args = objects[i].Body.IsCollisionTo(objects[i].Body, objects[j].Body);

                if (args.IsCollides)
                    CollisionEnter.Invoke(args);
            }
        });
    }
    
    // RECTANGLE WITH RECTANGLE

    internal static CollisionEventArgs PolygonToPolygon(IBody bodyA, IBody bodyB)
    {
        var args = new CollisionEventArgs(bodyA, bodyB);
        var rectangleBodyA = (PolygonBody)bodyA;
        var rectangleBodyB = (PolygonBody)bodyB;
        
        // TODO: CREATE PHYSICS

        return args;
    }
    
    // CIRCLE WITH CIRCLE
    
    internal static CollisionEventArgs CircleToCircle(IBody bodyA, IBody bodyB)
    {
        var args = new CollisionEventArgs(bodyA, bodyB);
        var circleBodyA = (EllipseBody)bodyA;
        var circleBodyB = (EllipseBody)bodyB;
        var normal = circleBodyA.Position - circleBodyA.Position;
        var radius = circleBodyA.Radius + circleBodyB.Radius;
        radius *= radius;
        var distance = normal.LengthSquared;

        if (distance > radius)
            return args;

        args.IsCollides = true;
        distance = MathF.Sqrt(distance);

        if (distance != 0)
        {
            args.Penetration = radius - distance;
            args.Normal = new Vector3(-1, 0 ,0);

            return args;
        }

        args.Penetration = circleBodyA.Radius;
        args.Normal = new Vector3(1, 0, 0);

        return args;
    }

    // RECTANGLE WITH CIRCLE

    internal static CollisionEventArgs CircleToPolygon(IBody circle, IBody rectangle)
    {
        throw new NotImplementedException();
    }
    
    internal static CollisionEventArgs PolygonToCircle(IBody rectangle, IBody circle)
    {
        throw new NotImplementedException();
    }

    private static void ResolveCollision(CollisionEventArgs args)
    {
        PositionCorrection(args);
        var (bodyA, bodyB) = (args.BodyA, args.BodyB);
        var velocityA = bodyA.Velocity + bodyA.Force;
        var velocityB = bodyB.Velocity + bodyB.Force;
        var difference = velocityB - velocityA;
        var velocityAlongNormal = Vector3.Dot(difference, args.Normal);

        if (velocityAlongNormal > 0)
            return;

        var e = MathF.Min(bodyA.Material.Restitution, bodyB.Material.Restitution);
        var jn = -(1 + e) * velocityAlongNormal / (bodyA.InverseMass + bodyB.InverseMass);
        var impulse = args.Normal * jn;
        
        lock (CollisionLocker)
        {
            bodyA.Velocity += -bodyA.InverseMass * impulse;
            bodyB.Velocity += bodyB.InverseMass * impulse;
        }

        difference = bodyB.Velocity - bodyA.Velocity;
        var tangent = difference - Vector3.Dot(difference, args.Normal) * args.Normal;
        tangent.NormalizeFast();

        var orientation1 = MathF.Pow(Vector3.Dot(bodyA.Centroid, tangent), 2) * bodyA.InverseInertia;
        var orientation2 = MathF.Pow(Vector3.Dot(bodyB.Centroid, tangent), 2) * bodyB.InverseInertia;
        var jt = -Vector3.Dot(difference, tangent);
        jt /= bodyA.InverseMass + bodyB.InverseMass + orientation1 + orientation2;

        var mu = (bodyA.Material.StaticFriction + bodyB.Material.StaticFriction) / 2;
        Vector3 frictionImpulse;

        if (MathF.Abs(jt) < jn * mu)
            frictionImpulse = jt * tangent;
        else
        {
            var dynamicFriction = bodyA.Material.DynamicFriction; 
            dynamicFriction = (dynamicFriction + bodyB.Material.DynamicFriction) / 2;
            frictionImpulse = -jn * tangent * dynamicFriction;
        }
        
        lock (CollisionLocker)
        {
            bodyA.Velocity += -bodyA.InverseMass * frictionImpulse;
            bodyB.Velocity += bodyB.InverseMass * frictionImpulse;
            bodyA.Torque += -bodyA.InverseInertia * frictionImpulse;
            bodyB.Torque += bodyB.InverseInertia * frictionImpulse;
        }
    }

    private static void PositionCorrection(CollisionEventArgs args)
    {
        const float percent = .2f;
        const float slop = .01f;
        var (bodyA, bodyB) = (args.BodyA, args.BodyB);
        var correction = MathF.Max(args.Penetration - slop, .0f) / (bodyA.InverseMass 
            + bodyB.InverseMass) * percent * args.Normal;
        var correction1 = -bodyA.InverseMass * correction;
        var correction2 = bodyB.InverseMass * correction;
        
        lock (CollisionLocker)
        {
            bodyA.Position += correction1;
            bodyB.Position += correction2;
        }
    }
}