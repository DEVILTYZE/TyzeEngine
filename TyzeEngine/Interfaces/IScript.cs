using OpenTK.Windowing.GraphicsLibraryFramework;
using TyzeEngine.GameStructure;

namespace TyzeEngine.Interfaces;

public interface IScript : IIdObject
{
    KeyboardState KeyboardState { get; }

    void Execute(TriggeredEventArgs args = null);
    internal void SetKeyboardState(KeyboardState state);
}