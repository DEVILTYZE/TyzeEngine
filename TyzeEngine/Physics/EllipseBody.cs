using System;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine.Physics;

public class EllipseBody : Body
{
    public float Radius { get; }
    
    public EllipseBody(IMaterial material, float radius = 1) : base(material)
    {
        Radius = radius;
        ComputeMass();
        AddMethod(typeof(EllipseBody), PhysicsGenerator.CircleToCircle);
        AddMethod(typeof(PolygonBody), PhysicsGenerator.CircleToPolygon);
    }

    public override CollisionEventArgs IsCollisionTo(IBody bodyA, IBody bodyB) 
        => CollisionMethods[bodyB.GetType()].Invoke(bodyA, bodyB);

    protected override Body CloneBody() => new EllipseBody(Material, Radius);

    protected sealed override void ComputeMass()
    {
        var r2 = Radius * Radius;
        SetMassAndInertia(MathF.PI * r2 * Material.Density, .5f * Mass * r2);
    }
}

