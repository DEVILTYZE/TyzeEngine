namespace TyzeEngine.Interfaces;

public interface IModel
{
    Uid Id { get; }
    string Directory { get; }
    string Name { get; }
    bool Loaded { get; }

    void Load();
    (float[], uint[]) GetVectorArray(IGameObject obj);
}