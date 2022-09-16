using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Materials;

namespace TyzeEngine.Interfaces;

public interface IBody : IDeepCloneable<IBody>
{
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
    Vector3 Force { get; set; }
    Vector3 GravityForce { get; set; }
    bool IsEnabled { get; set; }

    // PHYSICS
    void SetMassAndInertia(float mass, float inertia);
    internal void SetMaterial(IMaterial material);
    internal List<Vector3> GetVectors();
}