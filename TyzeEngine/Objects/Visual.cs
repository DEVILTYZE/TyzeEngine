using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

namespace TyzeEngine.Objects;

public class Visual : IVisual
{
    private IResource _texture;
    private Color4 _color;

    public Color4 Color { get => _color; set => _color = value; }
    public IResource Texture
    {
        get => _texture;
        set
        {
            _texture = value;
            _color = new Color4(_color.R, _color.G, _color.B, 0);
        }
    }
    public Visibility Visibility { get; set; } = Visibility.Visible;
    public BodyVisualType Type { get; protected internal set; } = BodyVisualType.Object;

    public IVisual Clone() => new Visual
    {
        Texture = Texture,
        Color = Color,
        Visibility = Visibility,
        Type = Type
    };
}