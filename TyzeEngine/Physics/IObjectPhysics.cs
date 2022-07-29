using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public interface IObjectPhysics
{
    IGameObject Object { get; set; }
    float InverseMass { get; }
    float Restitution { get; }
    Vector3 GravityForce { get; }
    Vector3 Velocity { get; }
    Dictionary<Type, Func<IGameObject, IGameObject, CollisionEventArgs>> CollisionMethods { get; }
    bool IsEnabled { get; set; }

    void AddMethod(Type physicsType, Func<IGameObject, IGameObject, CollisionEventArgs> method);
    CollisionEventArgs IsCollisionWith(IGameObject thisObj, IGameObject otherObj);
    void Impulse(Vector3 newVelocity);
    void Force();
    IObjectPhysics Clone();
}