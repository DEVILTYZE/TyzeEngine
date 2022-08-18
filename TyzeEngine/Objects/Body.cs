﻿using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine.Objects;

public abstract class Body : IBody
{
    private readonly Dictionary<Type, Func<IBody, IBody, CollisionEventArgs>> _collisionMethods;
    private readonly Dictionary<Uid, Vector3> _forces;

    public Vector3 Position { get; set; } = Constants.DefaultPosition;
    public Vector3 Scale { get; set; } = Constants.DefaultSize2D;
    public Vector3 Rotation { get; set; } = Constants.DefaultRotation;
    public Matrix3 RotationMatrix => Matrix3.Identity * Matrix3.CreateRotationX(Rotation.X) * Matrix3.CreateRotationY(
        Rotation.Y) * Matrix3.CreateRotationZ(Rotation.Z);
    public Vector4 Color { get; set; } = Constants.DefaultColor;
    public VisibilityType Visibility { get; set; } = VisibilityType.Visible;
    public BodyVisualType Visual { get; set; } = BodyVisualType.Color;

    public int Layer { get; set; }
    public IMaterial Material { get; }
    public float Mass { get; private set; }
    public float InverseMass { get; private set; }
    public float Inertia { get; private set; }
    public float InverseInertia { get; private set; }
    public Vector3 Centroid { get; set; } = Vector3.Zero;
    public Vector3 Torque { get; set; } = Vector3.Zero;
    public Vector3 Velocity { get; set; } = Vector3.Zero;
    public Vector3 AngularVelocity { get; set; } = Vector3.Zero;
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
    public IReadOnlyDictionary<Type, Func<IBody, IBody, CollisionEventArgs>> CollisionMethods => _collisionMethods;
    public bool IsEnabled { get; set; }

    protected Body(IMaterial material)
    {
        Material = material;
        SetMassAndInertia(0, 0);
        _forces = new Dictionary<Uid, Vector3>();
        _collisionMethods = new Dictionary<Type, Func<IBody, IBody, CollisionEventArgs>>();
    }

    public void SetColor(byte r, byte g, byte b, byte a) => Color = new Vector4(
        (float)r / byte.MaxValue, 
        (float)g / byte.MaxValue, 
        (float)b / byte.MaxValue,
        (float)a / byte.MaxValue
    );

    public void AddMethod(Type bodyType, Func<IBody, IBody, CollisionEventArgs> method)
    {
        if (_collisionMethods.ContainsKey(bodyType))
            _collisionMethods[bodyType] = method;
        else
            _collisionMethods.Add(bodyType, method);
    }

    public abstract CollisionEventArgs IsCollisionTo(IBody bodyA, IBody bodyB);

    public void AddForce(Uid id, Vector3 force)
    {
        if (_forces.ContainsKey(id))
            _forces[id] = force;
        else
            _forces.Add(id, force);
    }

    public void RemoveForce(Uid id) => _forces.Remove(id);
    
    public void SetMassAndInertia(float mass, float inertia)
    {
        Mass = mass;
        InverseMass = Mass == 0 ? 0 : 1 / Mass;
        Inertia = inertia;
        InverseInertia = Inertia == 0 ? 0 : 1 / Inertia;
    }

    public IBody Clone()
    {
        var obj = CloneBody();
        obj.Centroid = Centroid;
        obj.GravityForce = GravityForce;
        obj.Torque = Torque;
        obj.Mass = Mass;
        obj.InverseMass = InverseMass;
        obj.Inertia = Inertia;
        obj.InverseInertia = InverseInertia;
        obj.Position = Position;
        obj.Scale = Scale;
        obj.Rotation = Rotation;
        obj.Color = Color;
        obj.Visual = Visual;
        obj.Visibility = Visibility;

        return obj;
    }

    protected abstract Body CloneBody();

    protected abstract void ComputeMass();
}