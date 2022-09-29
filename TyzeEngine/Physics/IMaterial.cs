using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public interface IMaterial : IGameResource
{
    float Density { get; }
    float Restitution { get; }
    float StaticFriction { get; }
    float DynamicFriction { get; }
}