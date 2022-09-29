using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public abstract class Body : IBody
{
    public int CollisionLayer { get; set; }
    public int Dimension { get; }
    public IMaterial Material { get; }
    public float Mass { get; private set; }
    public float InverseMass { get; private set; }
    public float Inertia { get; private set; }
    public float InverseInertia { get; private set; }
    public Vector3 Centroid { get; set; }
    public Vector3 Torque { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 AngularVelocity { get; set; }
    public Vector3 Force { get; set; }
    public Vector3 GravityForce { get; set; }
    public bool TakesGravityForce { get; set; }
    public bool IsEnabled { get; set; } = true;

    protected Body(IMaterial material, int dimension)
    {
        if (dimension < 1)
            throw new Exception("Dimension less than zero.");

        Dimension = dimension;
        Material = material;
        SetMassAndInertia(0, 0);
    }

    public CollisionPoints TestCollision(ITransform transform, IBody body, ITransform bodyTransform) =>
        CollisionMethods.Invoke(this, transform, body, bodyTransform);

    public void SetMassAndInertia(float mass, float inertia)
    {
        Mass = mass;
        InverseMass = Mass == 0 ? 0 : 1 / Mass;
        Inertia = inertia;
        InverseInertia = Inertia == 0 ? 0 : 1 / Inertia;
    }

    public virtual IBody Clone(IBody obj = null)
    {
        if (obj is null)
            throw new ArgumentNullException(nameof(obj), "Clone object is null.");
        
        var body = (Body)obj;
        body.CollisionLayer = CollisionLayer;
        body.Centroid = Centroid;
        body.GravityForce = GravityForce;
        body.Torque = Torque;
        body.Mass = Mass;
        body.InverseMass = InverseMass;
        body.Inertia = Inertia;
        body.InverseInertia = InverseInertia;
        body.IsEnabled = IsEnabled;

        return body;
    }

    List<Vector3> IBody.GetVectors() => new() { Velocity, GravityForce, Force, Torque };
}