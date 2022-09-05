using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class Script : IScript
{
    public UId Id { get; set; } = new();

    public abstract void Prepare();
    
    public abstract void Execute();
    
    public abstract void FixedExecute();

    public static IScript FindOrDefault(string name)
    {
        var isFound = Game.Scripts.TryGetValue(name, out var value);

        return isFound ? value : null;
    }
}