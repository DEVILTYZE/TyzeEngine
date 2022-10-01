using TyzeEngine.GameStructure;

namespace TyzeEngine.Physics;

public class Rock : Material
{
    public Rock()
    {
        Density = .6f; 
        Restitution = .1f;
        StaticFriction = .6f;
        DynamicFriction = .42f;
    }
}

public class Wood : Material
{
    public Wood()
    {
        Density = .3f;
        Restitution = .2f;
        StaticFriction = .52f;
        DynamicFriction = .36f;
    }
}

public class Metal : Material
{
    public Metal()
    {
        Density = 1.2f; 
        Restitution = .05f;
        StaticFriction = .85f;
        DynamicFriction = .48f;
    }
}

public class BouncyBall : Material
{
    public BouncyBall()
    {
        Density = .3f;
        Restitution = .8f;
        StaticFriction = .4f;
        DynamicFriction = .13f;
    }
}

public class SuperBall : Material
{
    public SuperBall()
    {
        Density = .3f;
        Restitution = .95f;
        StaticFriction = 1;
        DynamicFriction = 1;
    }
}

public class Pillow : Material
{
    public Pillow()
    {
        Density = .1f;
        Restitution = .2f;
        StaticFriction = .84f;
        DynamicFriction = .62f;
    }
}

public class Static : Material
{
    public Static(bool withFriction)
    {
        Density = 0;
        Restitution = .4f;
        
        if (withFriction)
        {
            StaticFriction = 1;
            DynamicFriction = 1;
        }
        else
        {
            StaticFriction = 0;
            DynamicFriction = 0;
        }
    }
}