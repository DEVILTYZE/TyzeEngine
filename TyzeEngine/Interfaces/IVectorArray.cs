using System.Collections.Generic;

namespace TyzeEngine.Interfaces;

public interface IVectorArray
{
    ArrayType Type { get; }
    int Length { get; }

    void Add(float x);
    void Add(float x, float y);
    void Add(float x, float y, float z);
    void Add(float x, float y, float z, float t);
    IEnumerable<float> this[int index] { get; }
    internal float[] GetArray();
}