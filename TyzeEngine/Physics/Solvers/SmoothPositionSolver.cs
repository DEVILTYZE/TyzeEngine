using System;

namespace TyzeEngine.Physics.Solvers;

public class SmoothPositionSolver : CollisionSolver
{
    protected override void Solve(Manifold manifold)
    {
        const float percent = .8f;
        const float slop = .02f;
        var bodyA = manifold.ObjectA.Body;
        var bodyB = manifold.ObjectB.Body;
        var maxDepth = MathF.Max(manifold.Points.Penetration - slop, 0);
        var correction = maxDepth * (bodyA.InverseMass + bodyB.InverseMass) * percent * manifold.Points.Normal;
        var massSum = bodyA.Mass + bodyB.Mass;
        var ratioA = bodyA.Mass / massSum;
        var ratioB = bodyB.Mass / massSum;

        manifold.ObjectA.Transform.Position -= ratioA * bodyA.InverseMass * correction;
        manifold.ObjectB.Transform.Position += ratioB * bodyB.InverseMass * correction;
    }
}