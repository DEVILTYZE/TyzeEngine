using OpenTK.Windowing.GraphicsLibraryFramework;

namespace TyzeEngine.Interfaces;

public interface IScript : IIdObject
{
    KeyboardState KeyboardState { get; }

    void Execute();
    internal void SetKeyboardState(KeyboardState state);
}