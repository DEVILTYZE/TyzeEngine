using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class Script : IScript
{
    public IGameObject Object { get; }

    protected Script(IGameObject obj) => Object = obj;
    
    public abstract void Execute(TriggeredEventArgs args = null);
}