using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine.Physics;

public class CircleBody : Body
{
    public Vector3 Center { get; }
    public float Radius { get; }
    
    public CircleBody(IMaterial material, float volume, float inertia, bool isEnabled, 
        Vector3 center, float radius) 
        : base(material, volume, inertia, isEnabled)
    {
        Center = center;
        Radius = radius;
        AddMethod(typeof(CircleBody), PhysicsGenerator.CircleWithCircle);
    }

    public override CollisionEventArgs IsCollisionWith(IBody bodyA, IBody bodyB) 
        => CollisionMethods[bodyB.GetType()].Invoke(bodyA, bodyB);

    protected override IBody CloneThis()
        => new CircleBody(ObjectMaterial, Volume, Inertia, IsEnabled, Center, Radius);
}

