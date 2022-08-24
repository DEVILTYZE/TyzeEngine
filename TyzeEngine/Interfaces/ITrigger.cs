namespace TyzeEngine.Interfaces;

public interface ITrigger : IIdObject
{
    bool IsTriggered { get; set; }
    bool SaveStatus { get; }

    void OnTriggered();
}