using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public interface IBody
{
    IGameObject GameObject { get; set; }
    IMaterial ObjectMaterial { get; }
    float Mass { get; }
    float InverseMass { get; }
    float Volume { get; }
    float Restitution { get; }
    Vector3 Velocity { get; }
    Vector3 Force { get; }
    float GravityScale { get; }
    IReadOnlyDictionary<Type, Func<IGameObject, IGameObject, CollisionEventArgs>> CollisionMethods { get; }
    bool IsEnabled { get; set; }

    void AddMethod(Type physicsType, Func<IGameObject, IGameObject, CollisionEventArgs> method);
    CollisionEventArgs IsCollisionWith(IGameObject thisObj, IGameObject otherObj);
    void SetVelocity(Vector3 newVelocity);
    void AddVelocity(Vector3 additionalVelocity);
    void AddForce(Uid id, Vector3 force);
    void RemoveForce(Uid id);
    IBody Clone();
}