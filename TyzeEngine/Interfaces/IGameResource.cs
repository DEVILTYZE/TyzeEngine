namespace TyzeEngine.Interfaces;

public interface IGameResource
{
    UID Id { get; set; }

    internal void Remove();
}