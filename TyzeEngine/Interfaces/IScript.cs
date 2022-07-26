namespace TyzeEngine.Interfaces;

public interface IScript
{
    IGameObject Object { get; }

    void Execute(TriggeredEventArgs args);
}