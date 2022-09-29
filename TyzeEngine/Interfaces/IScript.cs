namespace TyzeEngine.Interfaces;

public interface IScript : IGameResource
{
    internal void PrepareScript();
    internal void ExecuteScript();
    internal void FixedExecuteScript();
}