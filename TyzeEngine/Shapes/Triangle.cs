using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine.Shapes;

public class Triangle : Model
{
    public Triangle(float size = 1) : base(GetModel(size))
    {
    }
    
    private static (List<Vector3>, List<Vector3>, IVectorArray, List<uint>) GetModel(float size)
    {
        var vertices = new List<Vector3> { new(-size, -size, 0), new(size, -size, 0), new(0, size, 0) };
        var indices = new List<uint> { 0, 1, 2 };
        var normals = new List<Vector3>(Enumerable.Repeat(Vector3.UnitZ, vertices.Count));

        return (vertices, normals, GetDefaultTexture(vertices), indices);
    }
}