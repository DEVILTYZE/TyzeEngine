using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public class Trigger : ITrigger
{
    private event ITrigger.TriggeredLoadEvent TriggeredLoad;
    private event ITrigger.TriggeredWorkEvent TriggeredWork;
    private readonly int _placeId;

    public Trigger(IScene scene, int placeId)
    {
        TriggeredLoad += scene.LoadPlace;
        _placeId = placeId;
    }

    public Trigger(IScript script) => TriggeredWork += script.Start;
    
    protected virtual void OnTriggeredLoad() => TriggeredLoad?.Invoke(_placeId);
    
    protected virtual void OnTriggeredWork() => TriggeredWork?.Invoke();
}