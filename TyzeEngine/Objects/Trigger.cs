using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public class Trigger : ITrigger
{
    private readonly int _placeId;

    protected event TriggerHandler Triggered;

    public Uid Id { get; } = new();
    public bool IsTriggered { get; set; }
    public bool SaveStatus { get; }

    private Trigger(bool notSave) => SaveStatus = !notSave;

    public Trigger(IScene scene, int placeId, bool notSave = false) : this(notSave)
    {
        _placeId = placeId;
        Triggered += scene.LoadPlace;
    }

    public Trigger(IScript script, bool notSave = false) : this(notSave)
    {
        _placeId = -1;
        Triggered += script.Execute;
    }

    public virtual void OnTriggered()
    {
        Triggered?.Invoke(new TriggeredEventArgs(_placeId == -1 ? null : _placeId));
        IsTriggered = true;
    }
}