using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Physics;

namespace TyzeEngine.Interfaces;

public interface IBody : IDeepCloneable<IBody>
{
    Vector3 Position { get; set; }
    Vector3 Scale { get; set; }
    Vector3 Rotation { get; set; }
    Matrix3 RotationMatrix { get; }
    Vector4 Color { get; set; }
    VisibilityType Visibility { get; set; }
    BodyVisualType Visual { get; set; }
    
    // PHYSICS
    ushort CollisionLayer { get; set; }
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
    bool IsEnabled { get; set; }

    void SetColor(byte r, byte g, byte b, byte a);
    
    // PHYSICS
    void AddForce(UId id, Vector3 force);
    void RemoveForce(UId id);
    void SetMassAndInertia(float mass, float inertia);
    internal IDictionary<UId, Vector3> GetForces();
    internal void SetMaterial(IMaterial material);
}