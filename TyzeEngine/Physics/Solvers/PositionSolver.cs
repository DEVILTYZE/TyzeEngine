using System;

namespace TyzeEngine.Physics.Solvers;

public class PositionSolver : CollisionSolver
{
    protected override void Solve(Manifold manifold)
    {
        var bodyA = manifold.ObjectA.Body;
        var bodyB = manifold.ObjectB.Body;
        var statA = bodyA.Mass == 0 ? 1 : 0;
        var statB = bodyB.Mass == 0 ? 1 : 0;
        var resolution = manifold.Points.Normal * manifold.Points.Penetration / MathF.Max(1, statA + statB);

        manifold.ObjectA.Transform.Position -= resolution * (1 - statA);
        manifold.ObjectB.Transform.Position += resolution * (1 - statB);
    }
}