using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

namespace TyzeEngine.Objects;

public class Visual : IVisual
{
    public Color4 Color { get; set; }
    public IResource Texture { get; set; }
    public ILight Light { get; set; }
    public Visibility Visibility { get; set; }
    public BodyVisualType Type { get; set; }

    public IVisual Clone() => new Visual
    {
        Color = Color,
        Texture = Texture,
        Light = Light,
        Visibility = Visibility,
        Type = Type
    };
}