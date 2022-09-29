using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Mathematics;

namespace TyzeEngine.Physics;

public class Simplex<T> where T : struct
{
    private readonly T[] _points = new T[4];

    public int Length { get; private set; }

    public Simplex()
    {
    }
    
    public Simplex(IEnumerable<T> enumerable)
    {
        var index = 0;
        
        foreach (var point in enumerable)
            _points[index++] = point;

        Length = index;
    }

    public void Add(T point)
    {
        for (var i = _points.Length - 1; i > 0; --i)
            _points[i] = _points[i - 1];

        _points[0] = point;
        Length = Math.Min(Length + 1, 4);
    }

    public T this[int index] => _points[index];

    public List<T> GetList() => _points[..Length].ToList();

    public override string ToString()
    {
        var sb = new StringBuilder("[");

        for (var i = 0; i < Length; ++i)
            sb.Append(_points[i] + ", ");

        return sb + "]";
    }
}