using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine.Shapes;

public class Rectangle : Model
{
    public Rectangle(float size = 1)
    {
        var vertices = new List<Vector3>
        {
            new(-size, -size, 0), new(size, -size, 0),
            new(size, size, 0), new(-size, size, 0)
        };
        var mesh = new Mesh
        {
            Vertices = vertices,
            Indices = new List<uint> { 0, 1, 2, 0, 2, 3 },
            Normals = new List<Vector3>(vertices),
            Texture = GetDefaultTexture(vertices)
        };
        ((IMesh)mesh).SetMesh();
        Meshes = new List<IMesh> { mesh };
    }
}