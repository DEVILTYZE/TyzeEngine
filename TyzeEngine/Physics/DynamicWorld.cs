﻿using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public class DynamicWorld : CollisionWorld, IPhysicsWorld
{
    private readonly Vector3 _gravityForce = new(0, -9.81f, 0);

    public void Step(IEnumerable<IGameObject> objects)
    {
        var list = objects.Where(obj => obj.Body is not null).ToList();

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
        => objects.ForEach(obj => obj.Body.Force += obj.Body.GravityForce * obj.Body.Mass);

    private static void MoveObject(IGameObject obj)
    {
        var angularAcceleration = obj.Body.Torque * obj.Body.InverseInertia;
        obj.Body.Torque = Vector3.Zero;
        obj.Body.AngularVelocity += angularAcceleration * FrameTimeState.FixedTime;
        obj.Transform.Rotation += obj.Body.AngularVelocity * FrameTimeState.FixedTime;
            
        var acceleration = obj.Body.Force * obj.Body.InverseMass;
        obj.Body.Force = Vector3.Zero;
        obj.Body.Velocity += acceleration * FrameTimeState.FixedTime;
        obj.Transform.Position += obj.Body.Velocity * FrameTimeState.FixedTime;
    }
}