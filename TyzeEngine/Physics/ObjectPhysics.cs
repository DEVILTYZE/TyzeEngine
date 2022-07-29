using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public abstract class ObjectPhysics : IObjectPhysics
{
    public IGameObject Object { get; set; }
    public float InverseMass { get; }
    public float Restitution { get; }
    public Vector3 GravityForce { get; }
    public Vector3 Velocity { get; private set; }
    public Dictionary<Type, Func<IGameObject, IGameObject, CollisionEventArgs>> CollisionMethods { get; }
    public bool IsEnabled { get; set; }

    private ObjectPhysics()
    {
        InverseMass = 0;
        Restitution = 0;
        GravityForce = Vector3.Zero;
        Velocity = Vector3.Zero;
        CollisionMethods = new Dictionary<Type, Func<IGameObject, IGameObject, CollisionEventArgs>>();
        IsEnabled = false;
    }

    protected ObjectPhysics(float mass, float restitution, Vector3 gravityForce, Vector3 velocity, bool isEnabled)
        : this()
    {
        InverseMass = mass == 0 ? mass : 1 / mass;
        Restitution = restitution;
        GravityForce = gravityForce;
        Velocity = velocity;
        IsEnabled = isEnabled;
    }

    public void AddMethod(Type physicsType, Func<IGameObject, IGameObject, CollisionEventArgs> method)
    {
        if (CollisionMethods.ContainsKey(physicsType))
            CollisionMethods[physicsType] = method;
        else
            CollisionMethods.Add(physicsType, method);
    }

    public abstract CollisionEventArgs IsCollisionWith(IGameObject thisObj, IGameObject otherObj);

    public void Impulse(Vector3 newVelocity) => Velocity += newVelocity;

    public void Force()
    {
        throw new NotImplementedException();
    }

    public abstract IObjectPhysics Clone();
}