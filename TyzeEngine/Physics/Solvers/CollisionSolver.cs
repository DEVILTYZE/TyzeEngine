using System.Collections.Generic;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics.Solvers;

public abstract class CollisionSolver : ICollisionSolver
{
    public void Solve(List<Manifold> manifolds) => manifolds.ForEach(Solve);

    protected abstract void Solve(Manifold manifold);
}