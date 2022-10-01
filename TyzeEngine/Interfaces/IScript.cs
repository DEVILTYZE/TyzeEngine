namespace TyzeEngine.Interfaces;

public interface IScript : IGameResource
{
    internal void InternalPrepare();
    internal void InternalExecute();
    internal void InternalFixedExecute();
}