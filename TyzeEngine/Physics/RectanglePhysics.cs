using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public class RectanglePhysics : ObjectPhysics
{
    private readonly Vector3 _d;
    private Vector3 _a, _b, _c;
    
    public float MaxX { get; private set; }
    public float MaxY { get; private set; }
    public float MinX { get; private set; }
    public float MinY { get; private set; }

    public RectanglePhysics() 
        : base(0, 0, Vector3.Zero, Vector3.Zero, false)
    {
    }
    
    public RectanglePhysics(float mass, float restitution, Vector3 gravityForce, Vector3 velocity, 
        bool isEnabled, IReadOnlyList<Vector3> points) 
        : base(mass, restitution, gravityForce, velocity, isEnabled)
    {
        _a = points[0];
        _b = points[1];
        _c = points[2];
        _d = points[3];
        CreateNewRectangle();
        CollisionMethods.Add(typeof(RectanglePhysics), PhysicsGenerator.RectangleWithRectangle);
    }
    
    public override CollisionEventArgs IsCollisionWith(IGameObject thisObj, IGameObject otherObj) 
        => CollisionMethods[otherObj.Physics.GetType()].Invoke(thisObj, otherObj);

    public override IObjectPhysics Clone() => new RectanglePhysics(1 / InverseMass, Restitution, GravityForce, 
        Velocity, IsEnabled, new[] { _a, _b, _c, _d });

    private void CreateNewRectangle()
    {
        if (_a + _c != _b + _d)
            (_c, _b) = (_b, _c);

        if (_a + _c != _b + _d)
            (_a, _b) = (_b, _a);
        
        MaxX = MathF.Max(MathF.Max(_a.X, _b.X), MathF.Max(_c.X, _d.X));
        MaxY = MathF.Max(MathF.Max(_a.Y, _b.Y), MathF.Max(_c.Y, _d.Y));
        MinX = MathF.Min(MathF.Min(_a.X, _b.X), MathF.Min(_c.X, _d.X));
        MinY = MathF.Min(MathF.Min(_a.Y, _b.Y), MathF.Min(_c.Y, _d.Y));
    }
}