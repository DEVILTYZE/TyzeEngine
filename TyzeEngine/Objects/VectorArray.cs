using System.Collections.Generic;
using System.Linq;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public class VectorArray : IVectorArray
{
    private readonly List<float> _list;
    
    public ArrayType Type { get; private set; }
    public int Length => _list.Count;

    public VectorArray()
    {
        _list = new List<float>();
        Type = ArrayType.Unknown;
    }

    public VectorArray(IEnumerable<float> array, ArrayType type)
    {
        _list = new List<float>(array);
        Type = type;
    }

    public void Add(float x)
    {
        if (Type != ArrayType.Unknown) 
            return;
        
        Type = ArrayType.OneDimension;
        _list.Add(x);
    }

    public void Add(float x, float y)
    {
        if (Type != ArrayType.Unknown) 
            return;
        
        Type = ArrayType.TwoDimensions;
        _list.AddRange(new[] { x, y });
    }

    public void Add(float x, float y, float z)
    {
        if (Type != ArrayType.Unknown) 
            return;
        
        Type = ArrayType.ThreeDimensions;
        _list.AddRange(new[] { x, y, z });
    }

    public void Add(float x, float y, float z, float w)
    {
        if (Type != ArrayType.Unknown) 
            return;
        
        Type = ArrayType.FourDimensions;
        _list.AddRange(new[] { x, y, z, w });
    }

    float[] IVectorArray.GetArray() => _list.ToArray();
}