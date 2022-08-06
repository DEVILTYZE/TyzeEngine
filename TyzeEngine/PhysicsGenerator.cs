using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine;

public static partial class PhysicsGenerator
{
    private static readonly object CollisionLocker = new();
    
    public static CollisionHandler CollisionEnter { get; set; } = ResolveCollision2D;
    public static CollisionHandler CollisionStay { get; set; }
    public static CollisionHandler CollisionExit { get; set; }

    public static void Collision2D(IReadOnlyList<IGameObject> objects)
    {
        Parallel.For(0, objects.Count - 1, i =>
        {
            for (var j = i + 1; j < objects.Count; ++j)
            {
                var args = objects[i].Body.IsCollisionWith(objects[i].Body, objects[j].Body);

                if (args.IsCollides)
                    CollisionEnter.Invoke(args);
            }
        });
    }

    private static void ResolveCollision2D(CollisionEventArgs args)
    {
        PositionCorrection(args);
        var (bodyA, bodyB) = (args.BodyA, args.BodyB);
        var velocityA = bodyA.Velocity + bodyA.Force;
        var velocityB = bodyB.Velocity + bodyB.Force;
        var difference = velocityB - velocityA;
        var velocityAlongNormal = Vector3.Dot(difference, args.Normal);

        if (velocityAlongNormal > 0)
            return;

        var e = MathF.Min(bodyA.ObjectMaterial.Restitution, bodyB.ObjectMaterial.Restitution);
        var jn = -(1 + e) * velocityAlongNormal / (bodyA.InverseMass + bodyB.InverseMass);
        var impulse = args.Normal * jn;
        
        lock (CollisionLocker)
        {
            bodyA.AddVelocity(-bodyA.InverseMass * impulse);
            bodyB.AddVelocity(bodyB.InverseMass * impulse);
        }

        difference = bodyB.Velocity - bodyA.Velocity;
        var tangent = difference - Vector3.Dot(difference, args.Normal) * args.Normal;
        tangent.NormalizeFast();

        var radius1 = Vector3.Zero;
        var radius2 = Vector3.Zero;
        var orientation1 = 0; // MathF.Pow(Vector3.Dot(radius1, tangent), 2) * bodyA.InverseInertia;
        var orientation2 = 0; // MathF.Pow(Vector3.Dot(radius2, tangent), 2) * bodyB.InverseInertia;
        var jt = -Vector3.Dot(difference, tangent);
        jt /= bodyA.InverseMass + bodyB.InverseMass + orientation1 + orientation2;

        var mu = (bodyA.ObjectMaterial.StaticFriction + bodyB.ObjectMaterial.StaticFriction) / 2;
        Vector3 frictionImpulse;

        if (MathF.Abs(jt) < jn * mu)
            frictionImpulse = jt * tangent;
        else
        {
            var dynamicFriction = bodyA.ObjectMaterial.DynamicFriction; 
            dynamicFriction = (dynamicFriction + bodyB.ObjectMaterial.DynamicFriction) / 2;
            frictionImpulse = -jn * tangent * dynamicFriction;
        }
        
        lock (CollisionLocker)
        {
            bodyA.AddVelocity(-bodyA.InverseMass * frictionImpulse);
            bodyB.AddVelocity(bodyB.InverseMass * frictionImpulse);
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
            bodyA.Translate(correction1.X, correction1.Y, correction1.Z);
            bodyB.Translate(correction2.X, correction2.Y, correction2.Z);
        }
    }
}