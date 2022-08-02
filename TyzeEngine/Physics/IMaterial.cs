namespace TyzeEngine.Physics;

public interface IMaterial
{
    float Density { get; }
    float Restitution { get; }
}