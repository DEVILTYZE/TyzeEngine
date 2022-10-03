using System.Collections.Generic;
using System.Threading.Tasks;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics.Solvers;

namespace TyzeEngine.Physics;

public class CollisionWorld
{
    protected CollisionHandler OnCollision { get; set; }

    public List<ICollisionSolver> Solvers { get; } = new()
    {
        new PositionSolver(),
        new SmoothPositionSolver(), 
        new ImpulseSolver()
    };

    protected void DetermineCollision(List<IGameObject> objects)
    {
        var collisions = new List<Manifold>();
        var triggers = new List<Manifold>();

        Parallel.For(0, objects.Count - 1, i =>
        {
            if (objects[i].CollisionState == CollisionState.NonCollides || objects[i].Visibility == Visibility.Collapsed)
                return;

            for (var j = i + 1; j < objects.Count; ++j)
            {
                var points = CollisionPoints.NonCollides;

                if (!IsCollides(objects[i], objects[j], ref points))
                    continue;

                if (objects[i].IsTrigger || objects[j].IsTrigger)
                    triggers.Add(new Manifold(objects[i], objects[j], points));
                else
                    collisions.Add(new Manifold(objects[i], objects[j], points));
            }
        });
        
        SolveCollisions(collisions);
        SendCollisionCallbacks(collisions);
        SendCollisionCallbacks(triggers);
    }

    private void SolveCollisions(List<Manifold> manifolds) => Solvers.ForEach(solver => solver.Solve(manifolds));

    private void SendCollisionCallbacks(List<Manifold> manifolds)
    {
        manifolds.ForEach(manifold =>
        {
            OnCollision?.Invoke(manifold);
            manifold.ObjectA.OnCollision?.Invoke(manifold);
            manifold.ObjectB.OnCollision?.Invoke(manifold);
        });
    }

    private static bool IsCollides(IGameObject objectA, IGameObject objectB, ref CollisionPoints points)
    {
        if (objectB.CollisionState == CollisionState.NonCollides || objectB.Visibility == Visibility.Collapsed)
            return false;
        
        if (objectA.CollisionState == CollisionState.Layer &&
            objectB.CollisionState == CollisionState.Layer &&
            objectA.Body.CollisionLayer == objectB.Body.CollisionLayer)
            return false;

        points = objectA.TestCollision(objectA.Transform, objectB, objectB.Transform);
        return points.IsCollides;
    }
}
