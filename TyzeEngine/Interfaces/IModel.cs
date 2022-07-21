using System;
using System.Linq;
using TyzeEngine.Objects;

namespace TyzeEngine.Interfaces;

public interface IModel
{
    string Name { get; }
    
    void ChangePosition(float x, float y, float z);
    void ChangeSize(float x, float y, float z);
    void ChangeColor(byte r, byte g, byte b, byte a);
    void ChangeTexture(IVectorArray array);
    (float[], float[]) GetVectorArray(bool withTexture = false);
    string ToString();

    internal static IModel GetByString(string str) // If you change something, then change everything.
    {
        var parts = str.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        var name = parts[0][8..];
        var type = (ArrayType)int.Parse(parts[5]);
        var floatArray = new float[][4];
        
        for (var i = 0; i < floatArray.Length; ++i)
            floatArray[i] = parts[i + 1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(float.Parse).ToArray();

        var saveData = new SaveModelData(
            name,
            new Vector3(floatArray[0][0], floatArray[0][1], floatArray[0][2]),
            new Vector3(floatArray[1][0], floatArray[1][1], floatArray[1][2]),
            new Vector4(floatArray[2][0], floatArray[2][1], floatArray[2][2], floatArray[2][3]),
            new VectorArray(floatArray[3], type));

        return new Model(saveData);
    }
}