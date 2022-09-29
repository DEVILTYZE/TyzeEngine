using System.Collections.Generic;

namespace TyzeEngine.Physics;

public interface ISolver
{
    void Solve(IReadOnlyList<CollisionEventArgs> collisions, float time);
}