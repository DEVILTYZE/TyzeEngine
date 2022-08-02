using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public abstract class Body : IBody
{
    private readonly Dictionary<Type, Func<IGameObject, IGameObject, CollisionEventArgs>> _collisionMethods;
    private readonly Dictionary<Uid, Vector3> _forces;
    
    public IGameObject GameObject { get; set; }
    public IMaterial ObjectMaterial { get; }
    public float Mass { get; }
    public float InverseMass { get; }
    public float Volume { get; }
    public float Restitution => ObjectMaterial.Restitution;
    public Vector3 Velocity { get; private set; }
    public Vector3 Force
    {
        get
        {
            var force = Vector3.Zero;

            foreach (var (_, currentForce) in _forces)
                force += currentForce;

            return force;
        }
    }
    public float GravityScale { get; }
    public IReadOnlyDictionary<Type, Func<IGameObject, IGameObject, CollisionEventArgs>> CollisionMethods =>
        _collisionMethods;
    public bool IsEnabled { get; set; }

    private Body()
    {
        ObjectMaterial = Material.Static;
        Mass = 0;
        InverseMass = 0;
        Volume = 0;
        GravityScale = 0;
        Velocity = Vector3.Zero;
        _forces = new Dictionary<Uid, Vector3>();
        _collisionMethods = new Dictionary<Type, Func<IGameObject, IGameObject, CollisionEventArgs>>();
        IsEnabled = false;
    }

    protected Body(IMaterial material, float volume, float gravityScale, bool isEnabled) : this()
    {
        ObjectMaterial = material;
        Volume = volume;
        Mass = material.Density * Volume;
        InverseMass = Mass == 0 ? Mass : 1 / Mass;
        GravityScale = gravityScale;
        IsEnabled = isEnabled;
    }

    public void AddMethod(Type physicsType, Func<IGameObject, IGameObject, CollisionEventArgs> method)
    {
        if (CollisionMethods.ContainsKey(physicsType))
            _collisionMethods[physicsType] = method;
        else
            _collisionMethods.Add(physicsType, method);
    }

    public abstract CollisionEventArgs IsCollisionWith(IGameObject thisObj, IGameObject otherObj);
    
    public void SetVelocity(Vector3 newVelocity) => Velocity = newVelocity;

    public void AddVelocity(Vector3 additionalVelocity) => Velocity += additionalVelocity;

    public void AddForce(Uid id, Vector3 force)
    {
        if (_forces.ContainsKey(id))
            _forces[id] = force;
        else
            _forces.Add(id, force);
    }

    public void RemoveForce(Uid id) => _forces.Remove(id);

    public abstract IBody Clone();
}