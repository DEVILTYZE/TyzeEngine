using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public class DynamicWorld : CollisionWorld, IPhysicsWorld
{
    private readonly Vector3 _gravityForce = new(0, -9.81f, 0);

    public void Step(IEnumerable<IGameObject> objects)
    {
        var list = objects.Where(obj => 
            obj.Body is not null && 
            obj.Body.IsEnabled && 
            obj.Visibility != Visibility.Collapsed).ToList();

        list.ForEach(obj =>
        {
            if (obj.Body.GravityForce == Vector3.Zero && obj.Body.TakesGravityForce)
                obj.Body.GravityForce = _gravityForce;
        });
        ApplyGravity(list);
        DetermineCollision(list);
        list.ForEach(MoveObject);
    }

    private static void ApplyGravity(List<IGameObject> objects)
        => objects.ForEach(obj => obj.Body.Force += obj.Body.GravityForce * obj.Body.InverseMass);

    private static void MoveObject(IGameObject obj)
    {
        var time = FrameTimeState.FixedTime;
        var angularAcceleration = obj.Body.Torque * obj.Body.InverseInertia;
        obj.Body.Torque = Vector3.Zero;
        obj.Body.AngularVelocity += angularAcceleration * time;
        obj.Transform.Rotation += obj.Body.AngularVelocity * time;
            
        var acceleration = obj.Body.Force * obj.Body.InverseMass;
        obj.Body.Force = Vector3.Zero;
        obj.Body.Velocity += acceleration * time;
        obj.Transform.Position += obj.Body.Velocity * time;
    }
}