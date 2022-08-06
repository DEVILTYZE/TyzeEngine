namespace TyzeEngine.Physics;

public class Material : IMaterial
{
    public float Density { get; }
    public float Restitution { get; }
    public float StaticFriction { get; }
    public float DynamicFriction { get; }

    public static IMaterial Rock => new Material(.6f, .1f, .6f, .42f);
    public static IMaterial Wood => new Material(.3f, .2f, .52f, .36f);
    public static IMaterial Metal => new Material(1.2f, .05f, .85f, .48f);
    public static IMaterial BouncyBall => new Material(.3f, .8f, .4f, .13f);
    public static IMaterial SuperBall => new Material(.3f, .95f, 1, 1);
    public static IMaterial Pillow => new Material(.1f, .2f, .84f, .62f);
    public static IMaterial Static => new Material(0, .4f, 0, 0);
    public static IMaterial StaticWithFriction => new Material(0, .4f, 1, 1);

    public Material(float density, float restitution, float staticFriction, float dynamicFriction)
    {
        Density = density;
        Restitution = restitution;
        StaticFriction = staticFriction;
        DynamicFriction = dynamicFriction;
    }
}