namespace TyzeEngine.Interfaces;

public interface IScript
{
    bool ExceptionWhileExecuting { get; }
    IGameObject Object { get; }

    void Start();
    void Pause();
    void Stop();
}