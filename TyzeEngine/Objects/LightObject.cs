using System;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public class LightObject : GameObject, ILightObject
{
    public LightType LightType { get; private set; }
    public Vector3 Direction { get; set; }
    
    public Vector3 Ambient { get; set; }
    public Vector3 Diffuse { get; set; }
    public Vector3 Specular { get; set; }

    public float Constant { get; set; }
    public float Linear { get; set; }
    public float Quadratic { get; set; }
    
    public float CutOff { get; set; }
    public float OuterCutOff { get; set; }
    
    public LightObject(LightType lightType)
    {
        LightType = lightType;
        VisualType = BodyVisualType.Light;
        Color = Color4.White;
        Direction = lightType == LightType.SpotLight ? -Vector3.UnitZ : Vector3.NormalizeFast(new Vector3(-1, -2, -1));
        
        Ambient = new Vector3(.1f, .1f, .1f);
        Diffuse = new Vector3(.8f, .8f, .8f);
        Specular = new Vector3(1, 1, 1);
        
        Constant = 1;
        Linear = .08f;
        Quadratic = .025f;
        
        CutOff = MathF.Cos(MathHelper.DegreesToRadians(12.5f));
        OuterCutOff = MathF.Cos(MathHelper.DegreesToRadians(17.5f));
    }

    public override ILightObject Clone(IGameObject obj = null)
    {
        var lightObj = (LightObject)base.Clone(obj ?? new LightObject(LightType));
        lightObj.LightType = LightType;
        lightObj.Color = Color;
        lightObj.Direction = Direction;
        lightObj.Ambient = Ambient;
        lightObj.Diffuse = Diffuse;
        lightObj.Specular = Specular;
        lightObj.Constant = Constant;
        lightObj.Linear = Linear;
        lightObj.Quadratic = Quadratic;
        lightObj.CutOff = CutOff;
        lightObj.OuterCutOff = OuterCutOff;

        return lightObj;
    }

    public override string ToString() =>
        $"Pos: {Transform.Position}; Dir: {Direction}\r\n" +
        $"Amb: {Ambient}; Dfs: {Diffuse}; Spc: {Specular}\r\n" +
        $"Const: {Constant}; Line: {Linear}; Quad: {Quadratic}\r\n" +
        $"Cut: {CutOff}; Out: {OuterCutOff}";

    string ILightObject.GetName(int[] count)
    {
        if (count[0] > Constants.MaxLightCount || count[1] > Constants.MaxLightCount)
            throw new Exception("Lights count bigger than max value.");
        
        return LightType switch
        {
            LightType.Direction => "dirLight",
            LightType.Point => $"pointLight[{count[0]++}]",
            LightType.SpotLight => $"spotLight[{count[1]++}]",
            _ => throw new ArgumentOutOfRangeException(nameof(LightType), "Light type error.")
        };
    }
}