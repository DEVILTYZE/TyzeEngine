using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Objects;

namespace TyzeEngine.Interfaces;

public interface IModel
{
    string Directory { get; }
    string Name { get; }
    bool LoadError { get; }
    Vector3 Position { get; }
    Vector3 Size { get; }
    Vector3 Rotation { get; }
    
    void Translate(float x, float y, float z);
    void Scale(float x, float y, float z);
    void Rotate(float x, float y, float z);
    void ChangeColor(byte r, byte g, byte b, byte a);
    void RemoveColor();
    void ChangeTexture(IVectorArray array);
    void AddDefaultTexture(bool withColor = false);
    void RemoveTexture();
    void ChangeVisibility(VisibilityType newType);
    (float[], uint[]) GetVectorArray();
    string ToString();

    internal static IModel GetByString(string str) // If you change something, then change everything.
    {
        const int count = 5;
        var parts = str.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        var name = parts[0][8..];
        var type = (ArrayType)int.Parse(parts[6]);
        var floatArray = new List<float[]>(count);

        for (var i = 0; i < count; ++i)
            floatArray.Add(parts[i + 1].Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(float.Parse).ToArray());

        var saveData = new SaveModelData(
            name,
            new Vector3(floatArray[0][0], floatArray[0][1], floatArray[0][2]),
            new Vector3(floatArray[1][0], floatArray[1][1], floatArray[1][2]),
            new Vector3(floatArray[2][0], floatArray[2][1], floatArray[2][2]),
            new Vector4(floatArray[3][0], floatArray[3][1], floatArray[3][2], floatArray[3][3]),
            new VectorArray(floatArray[4], type)
        );

        return new Model(saveData);
    }
}