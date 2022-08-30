using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine.Objects;

public abstract class Body : IBody
{
    private readonly Dictionary<UId, Vector3> _forces = new();
    private Quaternion _rotation;
    private Vector3 _scale;

    public Vector3 Position { get; set; } = Constants.DefaultPosition;
    public Vector3 Scale
    {
        get => _scale;
        set
        {
            var divide = Vector3.Zero;

            for (var i = 0; i < 3; ++i)
                if (_scale[i] != 0)
                    divide[i] = value[i] / _scale[i];

            _scale = value;
            RecomputeBodyParameters(divide);
        }
    }
    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;

            RotationMatrix = RotationMatrix * Matrix3.CreateRotationX(Rotation.X) * Matrix3.CreateRotationY(
                Rotation.Y) * Matrix3.CreateRotationZ(Rotation.Z);
        }
    }
    public Matrix3 RotationMatrix { get; private set; }
    public Vector4 Color { get; set; } = Constants.DefaultColor;
    public VisibilityType Visibility { get; set; } = VisibilityType.Visible;
    public BodyVisualType Visual { get; set; } = BodyVisualType.Color;

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
    public Vector3 GravityForce { get; set; }
    public bool IsEnabled { get; set; }

    protected Body(IMaterial material)
    {
        Scale = Constants.DefaultSize2D;
        Rotation = Constants.DefaultRotation;
        Material = material;
        SetMassAndInertia(0, 0);
    }

    public void SetColor(byte r, byte g, byte b, byte a) => Color = new Vector4(
        (float)r / byte.MaxValue, 
        (float)g / byte.MaxValue, 
        (float)b / byte.MaxValue,
        (float)a / byte.MaxValue
    );

    public void AddForce(UId id, Vector3 force)
    {
        if (_forces.ContainsKey(id))
            _forces[id] = force;
        else
            _forces.Add(id, force);
    }

    public void RemoveForce(UId id) => _forces.Remove(id);
    
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
        obj.Position = Position;
        obj._scale = Scale;
        obj._rotation = Rotation;
        obj.Color = Color;
        obj.Visual = Visual;
        obj.Visibility = Visibility;

        return obj;
    }

    IDictionary<UId, Vector3> IBody.GetForces() => _forces;
    void IBody.SetMaterial(IMaterial material) => Material = material;

    protected abstract Body DeepClone();

    protected abstract void ComputeMass();

    protected abstract void RecomputeBodyParameters(Vector3 newScale);
}