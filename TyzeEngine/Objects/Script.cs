using System;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class Script : IScript
{
    public UId Id { get; set; } = new();

    public abstract void Prepare();
    
    public abstract void Execute();
    
    public abstract void FixedExecute();

    void IGameResource.Remove()
    {
        if (Game.FrameScripts.Contains(this))
            Game.FrameScripts.Remove(this);
    }

    public static IScript Find(string name)
    {
        var isFound = Game.Scripts.TryGetValue(name, out var value);

        if (isFound)
            return value;

        throw new Exception("Script not found.");
    }
}