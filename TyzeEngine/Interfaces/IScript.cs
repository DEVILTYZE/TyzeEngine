using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TyzeEngine.Interfaces;

public interface IScript : IUIdObject
{
    void Prepare();
    void Execute();
}