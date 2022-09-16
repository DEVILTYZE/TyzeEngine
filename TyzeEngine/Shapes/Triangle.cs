using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Objects;

namespace TyzeEngine.Shapes;

public class Triangle : Model
{
    public Triangle(float size = 1)
    {
        SetModel(size);
        base.SetModel();
    }
    
    private void SetModel(float size)
    {
        Vertices = new List<Vector3> { new(-size, -size, 0), new(size, -size, 0), new(0, size, 0) };
        Indices = new List<uint> { 0, 1, 2 };
        Normals = new List<Vector3>(Enumerable.Repeat(Vector3.UnitZ, Vertices.Count));
        Texture = GetDefaultTexture(Vertices);
    }
}