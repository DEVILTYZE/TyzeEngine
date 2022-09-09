using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine.Shapes;

public class Point : Model
{
    public Point() : base(GetModel())
    {
    }

    private static (List<Vector3>, List<Vector3>, IVectorArray, List<uint>) GetModel()
    {
        var vertices = new List<Vector3> { new(0, 0, 0) };
        var indices = Enumerable.Repeat((uint)0, 1).ToList();
        var normals = new List<Vector3>(Enumerable.Repeat(Vector3.Zero, vertices.Count));

        return (vertices, normals, GetDefaultTexture(vertices), indices);
    }
}