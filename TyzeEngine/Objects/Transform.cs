using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

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
    public Matrix3 RotationMatrix => Matrix3.CreateFromQuaternion(Quaternion.FromEulerAngles(Rotation));
    public Vector4 Color { get; set; } = Constants.DefaultColor;
    public IResource Texture { get; set; }
    public Visibility Visibility { get; set; } = Visibility.Visible;
    public BodyVisualType Visual { get; set; } = BodyVisualType.Color;

    public Transform()
    {
        Scale = Constants.DefaultSize2D;
        Rotation = Constants.DefaultRotation;
    }
    
    public void SetColor(byte r, byte g, byte b, byte a) => Color = new Vector4(
        (float)r / byte.MaxValue, 
        (float)g / byte.MaxValue, 
        (float)b / byte.MaxValue,
        (float)a / byte.MaxValue
    );
    
    public ITransform Clone() => new Transform
    {
        Position = Position,
        _scale = Scale,
        Rotation = Rotation,
        Color = Color,
        Visibility = Visibility,
        Visual = Visual,
        Texture = Texture
    };
}