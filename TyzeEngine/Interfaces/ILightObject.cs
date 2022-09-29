using OpenTK.Mathematics;

namespace TyzeEngine.Interfaces;

public interface ILightObject : IGameObject
{ 
    LightType LightType { get; }
    Vector3 Direction { get; set; }

    Vector3 Ambient { get; set; }
    Vector3 Diffuse { get; set; }
    Vector3 Specular { get; set; }
    
    float Constant { get; set; }
    float Linear { get; set; }
    float Quadratic { get; set; }
    
    float CutOff { get; set; }
    float OuterCutOff { get; set; }

    internal string GetName(int[] count);
}