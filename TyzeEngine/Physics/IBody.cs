using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public interface IBody : IDeepCloneable<IBody>
{
    int CollisionLayer { get; set; }
    int Dimension { get; }
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
    bool TakesGravityForce { get; set; }
    bool IsEnabled { get; set; }
    
    CollisionPoints TestCollision(ITransform transform, IBody body, ITransform bodyTransform);
    void SetMassAndInertia(float mass, float inertia);
    internal List<Vector3> GetVectors();
}