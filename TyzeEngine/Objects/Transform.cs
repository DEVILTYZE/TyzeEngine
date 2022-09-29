using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public class Transform : ITransform
{
    private Vector3 _scale;
    
    public Vector3 Position { get; set; } = Constants.DefaultPosition;
    public Vector3 Scale
    {
        get => _scale;
        set
        {
            var divide = Vector3.Zero;

            for (var i = 0; i < 3; ++i)
                if (_scale[i] != 0)
                    divide[i] = value[i] / _scale[i];

            _scale = value;
            //((Body)Body).RecomputeBodyParameters(divide);
        }
    }
    public Vector3 Rotation { get; set; }
    public Matrix4 TranslationMatrix => Matrix4.CreateTranslation(Position);
    public Matrix4 ScaleMatrix => Matrix4.CreateScale(Scale);
    public Matrix4 RotationMatrix => Matrix4.CreateFromQuaternion(Quaternion.FromEulerAngles(Rotation));
    public Matrix4 ModelMatrix => Matrix4.Identity * ScaleMatrix * RotationMatrix * TranslationMatrix;

    public Transform()
    {
        Scale = Constants.DefaultSize;
        Rotation = Constants.DefaultRotation;
    }

    public ITransform Clone(ITransform obj = null) => (ITransform)MemberwiseClone();

    public override string ToString() => $"P: {Position}; S: {Scale}; R: {Rotation}";
}