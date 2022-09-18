using System;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class LightObject : GameObject, ILightObject
{
    public Vector3 Ambient { get; set; }
    public Vector3 Diffuse { get; set; }
    public Vector3 Specular { get; set; }

    protected LightObject()
    {
        ((Visual)Visual).Type = BodyVisualType.Light;
        Visual.Color = Color4.White;
        Ambient = new Vector3(.1f, .1f, .1f);
        Diffuse = new Vector3(.5f, .5f, .5f);
        Specular = new Vector3(.5f, .5f, .5f);
    }
}

public class DirectionLight : LightObject
{
    public Vector3 Direction { get; set; }

    public DirectionLight() => Direction = new Vector3(-2, -1, -1);

    protected override GameObject DeepClone() => new DirectionLight
    {
        Ambient = Ambient,
        Diffuse = Diffuse,
        Specular = Specular,
        Direction = Direction
    };
}

public class PointLight : LightObject
{
    public float Constant { get; set; }
    public float Linear { get; set; }
    public float Quadratic { get; set; }

    public PointLight()
    {
        Constant = 1;
        Linear = .08f;
        Quadratic = .025f;
    }
    
    protected override GameObject DeepClone() => new PointLight
    {
        Ambient = Ambient,
        Diffuse = Diffuse,
        Specular = Specular,
        Constant = Constant,
        Linear = Linear,
        Quadratic = Quadratic
    };
}

public class SpotLight : LightObject
{
    public Vector3 Direction { get; set; }
    public float CutOff { get; set; }
    public float OuterCutOff { get; set; }

    public SpotLight()
    {
        Direction = -Vector3.UnitZ;
        CutOff = MathF.Cos(MathHelper.DegreesToRadians(12.5f));
        OuterCutOff = MathF.Cos(MathHelper.DegreesToRadians(17.5f));
    }

    protected override GameObject DeepClone() => new SpotLight
    {
        Ambient = Ambient,
        Diffuse = Diffuse,
        Specular = Specular,
        Direction = Direction,
        CutOff = CutOff,
        OuterCutOff = OuterCutOff
    };
}