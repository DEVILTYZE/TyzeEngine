using System.Collections.Generic;

namespace TyzeEngine.Interfaces;

public interface ICollisionSolver
{
    void Solve(List<Manifold> collisions);
}