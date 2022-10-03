using System;
using OpenTK.Mathematics;

namespace TyzeEngine.Physics.Solvers;

public class ImpulseSolver : CollisionSolver
{
    protected override void Solve(Manifold manifold)
    {
        // IMPULSE
        var bodyA = manifold.ObjectA.Body;
        var bodyB = manifold.ObjectB.Body;
        var rVel = bodyB.Velocity - bodyA.Velocity;
        var velAlongNorm = Vector3.Dot(rVel, manifold.Points.Normal);
        
        if (velAlongNorm > 0)
            return;

        var e = bodyA.Material.Restitution * bodyB.Material.Restitution;
        var j = -((1 + e) * velAlongNorm) / (bodyA.InverseMass + bodyB.InverseMass);
        var impulse = j * manifold.Points.Normal;
        var massSum = bodyA.Mass + bodyB.Mass;
        var ratioA = bodyA.Mass / massSum;
        var ratioB = bodyB.Mass / massSum;
        
        bodyA.Velocity -= ratioA * bodyA.InverseMass * impulse;
        bodyB.Velocity += ratioB * bodyB.InverseMass * impulse;
        
        //FRICTION
        rVel = bodyB.Velocity - bodyA.Velocity;
        velAlongNorm = Vector3.Dot(rVel, manifold.Points.Normal);
        var tangent = rVel - velAlongNorm * manifold.Points.Normal;
        
        if (tangent.LengthFast > 0.0002f)
            tangent.NormalizeFast();

        var jt = -Vector3.Dot(rVel, tangent);
        var mu = PythagoreanSolve(bodyA.Material.StaticFriction, bodyB.Material.StaticFriction);
        var dfA = bodyA.Material.DynamicFriction;
        var dfB = bodyB.Material.DynamicFriction;
        jt /= (bodyA.InverseMass + bodyB.InverseMass);
        var friction = MathF.Abs(jt) < j * mu ? jt * tangent : -j * tangent * PythagoreanSolve(dfA, dfB);

        bodyA.Velocity -= bodyA.InverseMass * friction;
        bodyB.Velocity += bodyB.InverseMass * friction;
    }

    private static float PythagoreanSolve(float a, float b) => new Vector2(a, b).LengthFast;
}