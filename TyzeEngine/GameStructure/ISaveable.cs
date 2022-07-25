namespace TyzeEngine.GameStructure;

public interface ISaveable
{
    bool SaveStatus { get; }
    
    byte[] GetSaveData();
}