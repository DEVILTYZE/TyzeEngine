using OpenTK.Mathematics;

namespace TyzeEngine.Physics;

public class CollisionPoints
{
    public Vector3 A { get; set; }
    public Vector3 B { get; set; }
    public Vector3 Normal { get; set; }
    public float PenetrationDepth { get; set; }
    public bool IsCollides { get; set; }

    public static CollisionPoints NonCollides => new() { IsCollides = false }; 

    public CollisionPoints()
    {
        A = Vector3.Zero;
        B = Vector3.Zero;
        Normal = Vector3.Zero;
        PenetrationDepth = 0;
        IsCollides = false;
    }

    public override string ToString() => 
        $"A: {A}, B: {B}, N: {Normal}, D {PenetrationDepth}, " + (IsCollides ? "is collides" : "non collides");
}