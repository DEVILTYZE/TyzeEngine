namespace TyzeEngine.Interfaces;

public interface IScript : IGameResource
{
    void Prepare();
    void Execute();
    void FixedExecute();
}