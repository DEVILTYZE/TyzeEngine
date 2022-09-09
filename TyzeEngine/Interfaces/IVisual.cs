using OpenTK.Mathematics;
using TyzeEngine.Resources;

namespace TyzeEngine.Interfaces;

public interface IVisual : IDeepCloneable<IVisual>
{
    Color4 Color { get; set; }
    IResource Texture { get; set; }
    ILight Light { get; set; }
    Visibility Visibility { get; set; }
    BodyVisualType Type { get; set; }
}