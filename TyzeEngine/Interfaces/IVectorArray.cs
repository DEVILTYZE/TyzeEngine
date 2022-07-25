namespace TyzeEngine.Interfaces;

public interface IVectorArray
{
    ArrayType Type { get; }
    int Length { get; }
    
    void AddVector(float x);
    void AddVector(float x, float y);
    void AddVector(float x, float y, float z);
    void AddVector(float x, float y, float z, float t);
    internal float[] GetArray();
}