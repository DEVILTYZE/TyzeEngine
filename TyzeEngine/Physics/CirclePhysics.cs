using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public class CirclePhysics : ObjectPhysics
{
    public Vector3 Center { get; }
    public float Radius { get; }
    
    public CirclePhysics(float mass, float restitution, Vector3 gravityForce, Vector3 velocity, 
        bool isEnabled, Vector3 center, float radius) 
        : base(mass, restitution, gravityForce, velocity, isEnabled)
    {
        Center = center;
        Radius = radius;
        CollisionMethods.Add(typeof(CirclePhysics), PhysicsGenerator.CircleWithCircle);
    }

    public override CollisionEventArgs IsCollisionWith(IGameObject thisObj, IGameObject otherObj) 
        => CollisionMethods[otherObj.Physics.GetType()].Invoke(thisObj, otherObj);

    public override IObjectPhysics Clone() => new CirclePhysics(1 / InverseMass, Restitution, GravityForce, 
        Velocity, IsEnabled, Center, Radius);
}

