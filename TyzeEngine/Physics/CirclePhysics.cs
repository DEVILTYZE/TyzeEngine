using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public class CirclePhysics : Body
{
    public Vector3 Center { get; }
    public float Radius { get; }
    
    public CirclePhysics(IMaterial material, float volume, float gravityScale, bool isEnabled, Vector3 center, float radius) 
        : base(material, volume, gravityScale, isEnabled)
    {
        Center = center;
        Radius = radius;
        AddMethod(typeof(CirclePhysics), PhysicsGenerator.CircleWithCircle);
    }

    public override CollisionEventArgs IsCollisionWith(IGameObject thisObj, IGameObject otherObj) 
        => CollisionMethods[otherObj.Body.GetType()].Invoke(thisObj, otherObj);

    public override IBody Clone() => new CirclePhysics(ObjectMaterial, Volume, GravityScale, IsEnabled, Center, Radius);
}

