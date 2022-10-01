using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public class Transform : ITransform
{
    public Vector3 Position { get; set; } = Constants.DefaultPosition;
    public Vector3 Scale { get; set; }
    public Vector3 Rotation { get; set; }
    public Matrix4 ScaleMatrix => Matrix4.CreateScale(Scale);
    public Matrix4 RotationMatrix => Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(Rotation));
    public Matrix4 TranslationMatrix => Matrix4.CreateTranslation(Position);
    public Matrix4 ModelMatrix => Matrix4.Identity * ScaleMatrix * RotationMatrix * TranslationMatrix;

    public Transform()
    {
        Scale = Constants.DefaultSize;
        Rotation = Constants.DefaultRotation;
    }

    public ITransform Clone(ITransform obj = null) => (ITransform)MemberwiseClone();

    public override string ToString() => $"P: {Position}; S: {Scale}; R: {Rotation}";
}