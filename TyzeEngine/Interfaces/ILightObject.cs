using OpenTK.Mathematics;

namespace TyzeEngine.Interfaces;

public interface ILightObject
{ 
    Vector3 Ambient { get; set; }
    Vector3 Diffuse { get; set; }
    Vector3 Specular { get; set; }
}