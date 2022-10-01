using System.Collections.Generic;
using System.Threading.Tasks;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public class CollisionWorld
{
    protected CollisionHandler OnCollision { get; set; }
    
    public List<ISolver> Solvers { get; } = new();

    protected void DetermineCollision(List<IGameObject> objects)
    {
        var collisions = new List<CollisionEventArgs>();
        var triggers = new List<CollisionEventArgs>();
        var locker = new object();
        
        Parallel.For(0, objects.Count - 1, i =>
        {
            for (var j = i + 1; j < objects.Count; ++j)
            {
                if (objects[i].Body.CollisionLayer == objects[j].Body.CollisionLayer)
                    continue;

                var points = objects[i].TestCollision(objects[i].Transform, objects[j], objects[j].Transform);

                if (!points.IsCollides) 
                    continue;
                
                if (objects[i].IsTrigger || objects[j].IsTrigger) lock (locker)
                    triggers.Add(new CollisionEventArgs(objects[i], objects[j], points));
                else lock (locker)
                    collisions.Add(new CollisionEventArgs(objects[i], objects[j], points));
            }
        });
        
        SolveCollisions(collisions);
        SendCollisionCallback(collisions);
        SendCollisionCallback(triggers);
    }

    private void SolveCollisions(IReadOnlyList<CollisionEventArgs> collisions) =>
        Solvers.ForEach(solver => solver.Solve(collisions, FrameTimeState.RenderTime));

    private void SendCollisionCallback(List<CollisionEventArgs> collisions)
    {
        collisions.ForEach(args =>
        {
            OnCollision?.Invoke(args);
            args.ObjectA.OnCollision?.Invoke(args);
            args.ObjectB.OnCollision?.Invoke(args);
        });
    }
}
