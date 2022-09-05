using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine.Shapes;

public class Cube : Model
{
    public Cube(float size = 1) : base(GetModel(size))
    {
    }
    
    private static (List<Vector3>, uint[], IVectorArray) GetModel(float size)
    {
        var vertices = GetVertices(size);
        var indices = new uint[]
        {
            0, 1, 2, 0, 2, 3, 0, 4, 5, 1, 5, 6,
            1, 6, 2, 3, 2, 6, 3, 6, 7, 4, 0, 3,
            4, 3, 7, 5, 1, 0, 5, 4, 7, 5, 7, 6
        };
        var texture = new VectorArray(new[] { 0f, 0, 1, 0, 1, 1, 0, 1, 1, 0, 0, 0, 0, 1, 1, 1 }, ArrayType.TwoDimensions);

        return (vertices, indices, texture);
    }

    private static List<Vector3> GetVertices(float size)
    {
        var rect = new Vector3[]
        {
            new(-size, -size, size), new(size, -size, size), 
            new(size, size, size), new(-size, size, size)
        };
        var backRect = new Vector3[rect.Length];

        for (var i = 0; i < rect.Length; ++i)
            backRect[i] = new Vector3(rect[i].X, rect[i].Y, -size);

        return rect.Concat(backRect).ToList();
    }
}