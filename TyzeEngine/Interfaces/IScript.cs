using OpenTK.Windowing.GraphicsLibraryFramework;
using TyzeEngine.GameStructure;

namespace TyzeEngine.Interfaces;

public interface IScript
{
    IGameObject Object { get; }
    KeyboardState KeyboardState { get; }
    IScene Scene { get; }

    void Execute(TriggeredEventArgs args = null);
    internal void AddArgs(KeyboardState state, IScene scene);
}