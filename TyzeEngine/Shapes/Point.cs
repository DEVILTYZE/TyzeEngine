using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Objects;

namespace TyzeEngine.Shapes;

public class Point : Model
{
    public Point()
    {
        SetModel();
        base.SetModel();
    }

    private new void SetModel()
    {
        Vertices = new List<Vector3> { new(0, 0, 0) };
        Indices = Enumerable.Repeat((uint)0, 1).ToList();
        Normals = new List<Vector3>(Enumerable.Repeat(Vector3.Zero, Vertices.Count));
        Texture = GetDefaultTexture(Vertices);
    }
}