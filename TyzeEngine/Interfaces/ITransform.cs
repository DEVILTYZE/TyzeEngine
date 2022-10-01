using OpenTK.Mathematics;

namespace TyzeEngine.Interfaces;

public interface ITransform : IDeepCloneable<ITransform>
{
    Vector3 Position { get; set; } 
    Vector3 Scale { get; set; }
    Vector3 Rotation { get; set; }
    Matrix4 ScaleMatrix { get; }
    Matrix4 RotationMatrix { get; }
    Matrix4 TranslationMatrix { get; }
    Matrix4 ModelMatrix { get; }
}