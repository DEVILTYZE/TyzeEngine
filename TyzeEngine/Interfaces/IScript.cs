namespace TyzeEngine.Interfaces;

public interface IScript : IUIdObject
{
    void Prepare();
    void Execute();
    void FixedExecute();
}