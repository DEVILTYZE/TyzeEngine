using System.Collections.Generic;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class Script : IScript
{
    public IGameObject Object { get; }
    public KeyboardState KeyboardState { get; private set; }
    public IScene Scene { get; private set; }

    protected Script(IGameObject obj)
    {
        Object = obj;
        Object.Scripts.Add(this);
    }
    
    public abstract void Execute(TriggeredEventArgs args = null);
    void IScript.AddArgs(KeyboardState state, IScene scene)
    {
        KeyboardState = state;
        Scene = scene;
    }
}