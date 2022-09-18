using OpenTK.Mathematics;
using TyzeEngine.Objects;
using TyzeEngine.Resources;

namespace TyzeEngine.Interfaces;

public interface IVisual : IDeepCloneable<IVisual>
{
    Color4 Color { get; set; }
    IResource Texture { get; set; }
    Visibility Visibility { get; set; }
    BodyVisualType Type { get; }
}