using OpenTK.Mathematics;
using TyzeEngine.Resources;

namespace TyzeEngine.Interfaces;

public interface ITransform : IDeepCloneable<ITransform>
{
    Vector3 Position { get; set; } 
    Vector3 Scale { get; set; }
    Vector3 Rotation { get; set; }
    Matrix3 RotationMatrix { get; }
    Vector4 Color { get; set; }
    IResource Texture { get; set; }
    Visibility Visibility { get; set; }
    BodyVisualType Visual { get; set; }

    void SetColor(byte r, byte g, byte b, byte a);
}