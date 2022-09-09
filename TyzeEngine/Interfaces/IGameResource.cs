namespace TyzeEngine.Interfaces;

public interface IGameResource
{
    UId Id { get; set; }

    internal void Remove();
}