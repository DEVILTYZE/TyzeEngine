using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public class Light : ILight
{
    public float Ambient { get; }
    public float Specular { get; }
    public float Shininess { get; }

    public static ILight Default => new Light(.1f, .5f, 32);

    public Light(float ambient, float specular, float shininess)
    {
        Ambient = ambient;
        Specular = specular;
        Shininess = shininess;
    }
}