using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Materials;

namespace TyzeEngine.Objects;

public abstract class Body : IBody
{
    public ushort CollisionLayer { get; set; }
    public IMaterial Material { get; private set; }
    public float Mass { get; private set; }
    public float InverseMass { get; private set; }
    public float Inertia { get; private set; }
    public float InverseInertia { get; private set; }
    public Vector3 Centroid { get; set; } = Vector3.Zero;
    public Vector3 Torque { get; set; } = Vector3.Zero;
    public Vector3 Velocity { get; set; } = Vector3.Zero;
    public Vector3 AngularVelocity { get; set; }
    public Vector3 Force { get; set; }
    public Vector3 GravityForce { get; set; }
    public bool IsEnabled { get; set; }

    protected Body(IMaterial material)
    {
        Material = material;
        SetMassAndInertia(0, 0);
    }
    
    public void SetMassAndInertia(float mass, float inertia)
    {
        Mass = mass;
        InverseMass = Mass == 0 ? 0 : 1 / Mass;
        Inertia = inertia;
        InverseInertia = Inertia == 0 ? 0 : 1 / Inertia;
    }

    public IBody Clone()
    {
        var obj = DeepClone();
        obj.Centroid = Centroid;
        obj.GravityForce = GravityForce;
        obj.Torque = Torque;
        obj.Mass = Mass;
        obj.InverseMass = InverseMass;
        obj.Inertia = Inertia;
        obj.InverseInertia = InverseInertia;

        return obj;
    }

    void IBody.SetMaterial(IMaterial material) => Material = material;

    List<Vector3> IBody.GetVectors() => new() { Velocity, GravityForce, Force, Torque };

    protected abstract Body DeepClone();

    protected abstract void ComputeMass();

    internal abstract void RecomputeBodyParameters(Vector3 newScale);
}