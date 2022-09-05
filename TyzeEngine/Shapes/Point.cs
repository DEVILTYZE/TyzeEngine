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

    private static (List<Vector3>, uint[], IVectorArray) GetModel()
    {
        var vertices = new List<Vector3> { new(0, 0, 0) };
        var indices = Enumerable.Repeat((uint)0, 1).ToArray();

        return (vertices, indices, GetDefaultTexture(vertices));
    }
}