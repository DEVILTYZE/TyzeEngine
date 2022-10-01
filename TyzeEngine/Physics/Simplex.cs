using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        Length = Math.Min(Length + 1, 4);
        
        for (var i = Length - 1; i > 0; --i)
            _points[i] = _points[i - 1];

        _points[0] = point;
    }

    public T this[int index] => _points[index];

    public List<T> GetList() => _points[..Length].ToList();

    public override string ToString()
    {
        var sb = new StringBuilder("[");

        for (var i = 0; i < Length - 1; ++i)
            sb.Append(_points[i] + ", ");
        
        if (Length > 0)
            sb.Append(_points[Length - 1]);
        
        return sb + "]";
    }
}