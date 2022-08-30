using OpenTK.Windowing.GraphicsLibraryFramework;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class Script : IScript
{
    public UId Id { get; set; } = new();
    public KeyboardState KeyboardState { get; private set; }

    public abstract void Execute();

    public static IScript FindOrDefault(string name)
    {
        var isFound = Game.Instance.Scripts.TryGetValue(name, out var value);

        return isFound ? value : null;
    }
    
    void IScript.SetKeyboardState(KeyboardState state) => KeyboardState = state;
}