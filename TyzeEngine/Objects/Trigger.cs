using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public sealed class Trigger : ITrigger
{
    private event TriggerHandler Triggered;

    public UId Id { get; set; } = new();
    public bool IsTriggered { get; set; }
    public bool SaveStatus { get; }

    public Trigger(bool notSave = true) => SaveStatus = !notSave;

    public void AddScript(IScript script)
    {
        Triggered += script.Prepare;
        Triggered += script.Execute;
    }

    public void RemoveScript(IScript script)
    {
        Triggered -= script.Execute;
        Triggered -= script.Prepare;
    }

    public void OnTriggered()
    {
        Triggered?.Invoke();
        IsTriggered = true;
    }

    public static ITrigger FindOrDefault(string name)
    {
        var isFound = Game.Triggers.TryGetValue(name, out var value);

        return isFound ? value : null;
    }
}