namespace TyzeEngine.Interfaces;

public interface ITrigger : IUIdObject
{
    bool IsTriggered { get; set; }
    bool SaveStatus { get; }

    void AddScript(IScript script);
    void RemoveScript(IScript script);
    void OnTriggered();
}