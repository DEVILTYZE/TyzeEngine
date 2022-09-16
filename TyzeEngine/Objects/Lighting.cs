namespace TyzeEngine.Objects;

public struct Lighting
{
    public float Ambient { get; }
    public float Specular { get; }
    public float Shininess { get; }

    public static Lighting Default => new(.1f, .5f, 32);

    public Lighting(float ambient, float specular, float shininess)
    {
        Ambient = ambient;
        Specular = specular;
        Shininess = shininess;
    }
}