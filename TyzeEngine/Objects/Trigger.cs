using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public sealed class Trigger : ITrigger
{
    private readonly int _placeId = -1;
    private event TriggerHandler Triggered;

    public UId Id { get; set; } = new();
    public bool IsTriggered { get; set; }
    public bool SaveStatus { get; }

    private Trigger(bool notSave) => SaveStatus = !notSave;

    public Trigger(IScene scene, int placeId, bool notSave = false) : this(notSave)
    {
        _placeId = placeId;
        Triggered += scene.LoadPlace;
    }

    public Trigger(IScript script, bool notSave = false) : this(notSave) => Triggered += script.Execute;
    
    public void OnTriggered()
    {
        Triggered?.Invoke(new TriggeredEventArgs(_placeId));
        IsTriggered = true;
    }

    public static ITrigger Find(string name)
    {
        var isFound = Game.Triggers.TryGetValue(name, out var value);

        return isFound ? value : null;
    }
}