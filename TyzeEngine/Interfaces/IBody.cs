using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Physics;

namespace TyzeEngine.Interfaces;

public interface IBody
{
    IGameObject GameObject { get; set; }
    Vector3 Position { get; }
    Vector3 Size { get; }
    Vector3 Rotation { get; }
    Vector4 Color { get; }
    bool HasTexture { get; }
    VisibilityType Visibility { get; set; }
    
    // PHYSICS
    IMaterial ObjectMaterial { get; }
    float Mass { get; }
    float InverseMass { get; }
    float Inertia { get; }
    float InverseInertia { get; }
    float Volume { get; }
    Vector3 Velocity { get; }
    Vector3 Force { get; }
    Vector3 GravityForce { get; set; }
    IReadOnlyDictionary<Type, Func<IBody, IBody, CollisionEventArgs>> CollisionMethods { get; }
    bool IsEnabled { get; set; }

    void SetPosition(float x, float y, float z);
    void SetScale(float x, float y, float z);
    void SetRotation(float x, float y, float z);
    void Translate(float x, float y, float z);
    void Scale(float x, float y, float z);
    void Rotate(float x, float y, float z);
    void SetColor(byte r, byte g, byte b, byte a);
    void SetColor(Vector4 rgba);
    void RemoveColor();
    void SetTextureStatus(bool isEnabled, bool withColor = false);
    
    // PHYSICS
    void AddMethod(Type physicsType, Func<IBody, IBody, CollisionEventArgs> method);
    CollisionEventArgs IsCollisionWith(IBody bodyA, IBody bodyB);
    void SetVelocity(Vector3 newVelocity);
    void AddVelocity(Vector3 additionalVelocity);
    void AddForce(Uid id, Vector3 force);
    void RemoveForce(Uid id);
    IBody Clone();
}