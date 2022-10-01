using System.Collections.Generic;

namespace TyzeEngine.Interfaces;

public interface ISolver
{
    void Solve(IReadOnlyList<CollisionEventArgs> collisions, float time);
}