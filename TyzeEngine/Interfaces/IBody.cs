using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Physics;

namespace TyzeEngine.Interfaces;

public interface IBody
{
    IGameObject GameObject { get; set; }
    Vector3 Position { get; set; }
    Vector3 Scale { get; set; }
    Vector3 Rotation { get; set; }
    Vector4 Color { get; set; }
    VisibilityType Visibility { get; set; }
    BodyVisualType Visual { get; set; }
    
    // PHYSICS
    IMaterial Material { get; }
    float Mass { get; }
    float InverseMass { get; }
    float Inertia { get; }
    float InverseInertia { get; }
    Vector3 Centroid { get; set; }
    Vector3 Torque { get; set; }
    Vector3 Velocity { get; set; }
    Vector3 AngularVelocity { get; set; }
    Vector3 Force { get; }
    Vector3 GravityForce { get; set; }
    IReadOnlyDictionary<Type, Func<IBody, IBody, CollisionEventArgs>> CollisionMethods { get; }

    void SetColor(byte r, byte g, byte b, byte a);
    
    // PHYSICS
    void AddMethod(Type physicsType, Func<IBody, IBody, CollisionEventArgs> method);
    CollisionEventArgs IsCollisionTo(IBody bodyA, IBody bodyB);
    void AddForce(Uid id, Vector3 force);
    void RemoveForce(Uid id);
    void SetMassAndInertia(float mass, float inertia);
    IBody Clone();
}