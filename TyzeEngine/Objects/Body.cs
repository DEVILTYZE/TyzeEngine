using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine.Objects;

public abstract class Body : IBody
{
    private readonly Dictionary<Type, Func<IBody, IBody, CollisionEventArgs>> _collisionMethods;
    private readonly Dictionary<Uid, Vector3> _forces;
    
    public IGameObject GameObject { get; set; }
    public Vector3 Position { get; private set; }
    public Vector3 Size { get; private set; }
    public Vector3 Rotation { get; private set; }
    public Vector4 Color { get; private set; }
    public bool HasTexture { get; private set; }
    public VisibilityType Visibility { get; set; }
    
    public IMaterial ObjectMaterial { get; }
    public float Mass { get; }
    public float InverseMass { get; }
    public float Inertia { get; }
    public float InverseInertia { get; }
    public float Volume { get; }
    public Vector3 Velocity { get; private set; }
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

    private Body()
    {
        Position = Constants.DefaultPosition;
        Size = Constants.DefaultSize2D;
        Rotation = Constants.DefaultRotation;
        Color = Constants.DefaultColor;
        HasTexture = false;
        Visibility = VisibilityType.Visible;
        
        ObjectMaterial = Material.Static;
        Mass = 0;
        InverseMass = 0;
        Volume = 0;
        Velocity = Vector3.Zero;
        _forces = new Dictionary<Uid, Vector3>();
        _collisionMethods = new Dictionary<Type, Func<IBody, IBody, CollisionEventArgs>>();
        IsEnabled = false;
    }

    protected Body(IMaterial material, float volume, float inertia, bool isEnabled) : this()
    {
        ObjectMaterial = material;
        Volume = volume;
        Mass = material.Density * Volume;
        Inertia = inertia;
        InverseMass = Mass == 0 ? Mass : 1 / Mass;
        InverseInertia = Inertia == 0 ? Inertia : 1 / Inertia;
        IsEnabled = isEnabled;
    }
    
    public void SetPosition(float x, float y, float z) => Position = new Vector3(x, y, z);

    public void SetScale(float x, float y, float z)
    {
        if (x < 0 || y < 0 || z < 0)
            return;

        Size = new Vector3(x, y, z);
    }

    public void SetRotation(float x, float y, float z) => Rotation = new Vector3(x, y, z);

    public void Translate(float x, float y, float z) => SetPosition(Position.X + x, Position.Y + y, Position.Z + z);

    public void Scale(float x, float y, float z) => SetScale(Size.X * x, Size.Y * y, Size.Z * z);

    public void Rotate(float x, float y, float z) => SetRotation(Rotation.X + x, Rotation.Y + y, Rotation.Z + z);

    public void SetColor(byte r, byte g, byte b, byte a) => Color = new Vector4(
        (float)r / byte.MaxValue, 
        (float)g / byte.MaxValue, 
        (float)b / byte.MaxValue,
        (float)a / byte.MaxValue
    );

    public void SetColor(Vector4 rgba) => Color = rgba;

    public void RemoveColor() => Color = Constants.NullColor;
    
    public void SetTextureStatus(bool isEnabled, bool withColor = false)
    {
        HasTexture = isEnabled;
        
        if (!withColor)
            RemoveColor();
    }

    public void AddMethod(Type physicsType, Func<IBody, IBody, CollisionEventArgs> method)
    {
        if (CollisionMethods.ContainsKey(physicsType))
            _collisionMethods[physicsType] = method;
        else
            _collisionMethods.Add(physicsType, method);
    }

    public abstract CollisionEventArgs IsCollisionWith(IBody bodyA, IBody bodyB);
    
    public void SetVelocity(Vector3 newVelocity) => Velocity = newVelocity;

    public void AddVelocity(Vector3 additionalVelocity) => Velocity += additionalVelocity;

    public void AddForce(Uid id, Vector3 force)
    {
        if (_forces.ContainsKey(id))
            _forces[id] = force;
        else
            _forces.Add(id, force);
    }

    public void RemoveForce(Uid id) => _forces.Remove(id);

    public IBody Clone()
    {
        var body = CloneThis();
        body.GravityForce = GravityForce;
        body.SetPosition(Position.X, Position.Y, Position.Z);
        body.SetScale(Size.X, Size.Y, Size.Z);
        body.SetRotation(Rotation.X, Rotation.Y, Rotation.Z);
        body.SetColor(Color);
        body.SetTextureStatus(HasTexture, Math.Abs(Color.W - 1) < float.Epsilon);

        return body;
    }
    
    protected abstract IBody CloneThis();
}