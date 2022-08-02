namespace TyzeEngine.Physics;

public class Material : IMaterial
{
    public float Density { get; }
    public float Restitution { get; }

    public static IMaterial Rock => new Material(.6f, .1f);
    public static IMaterial Wood => new Material(.3f, .2f);
    public static IMaterial Metal => new Material(1.2f, .05f);
    public static IMaterial BouncyBall => new Material(.3f, .8f);
    public static IMaterial SuperBall => new Material(.3f, .95f);
    public static IMaterial Pillow => new Material(.1f, .2f);
    public static IMaterial Static => new Material(0, .4f);

    public Material(float density, float restitution)
    {
        Density = density;
        Restitution = restitution;
    }
}