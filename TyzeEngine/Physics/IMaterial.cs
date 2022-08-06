namespace TyzeEngine.Physics;

public interface IMaterial
{
    float Density { get; }
    float Restitution { get; }
    float StaticFriction { get; }
    float DynamicFriction { get; }
}