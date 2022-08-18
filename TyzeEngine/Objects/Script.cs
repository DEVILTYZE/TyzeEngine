using OpenTK.Windowing.GraphicsLibraryFramework;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class Script : IScript
{
    public IGameObject GameObject { get; }
    public KeyboardState KeyboardState { get; private set; }
    public IScene Scene { get; private set; }

    protected Script(IGameObject obj)
    {
        GameObject = obj;
        GameObject.Scripts.Add(this);
    }
    
    public abstract void Execute(TriggeredEventArgs args = null);
    
    void IScript.AddArgs(KeyboardState state, IScene scene)
    {
        KeyboardState = state;
        Scene = scene;
    }
}