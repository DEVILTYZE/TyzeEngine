namespace TyzeEngine.Interfaces;

public interface IScript
{
    IGameObject Object { get; }

    void Execute(EventTriggeredArgs args);
}