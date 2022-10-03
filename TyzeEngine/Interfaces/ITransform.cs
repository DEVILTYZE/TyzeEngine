using OpenTK.Mathematics;
using TyzeEngine.Physics;

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

    public static ITransform operator *(ITransform transform, float number) => new Transform
    {
        Position = transform.Position * number,
        Scale = transform.Scale * number,
        Rotation = transform.Rotation * number
    };

    public static ITransform operator +(ITransform transform1, ITransform transform2) => new Transform
    {
        Position = transform1.Position + transform2.Position,
        Scale = transform1.Scale + transform2.Scale,
        Rotation = transform1.Rotation + transform2.Rotation
    };
}