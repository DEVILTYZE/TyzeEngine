using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine.Shapes;

public class Point : Model
{
    public Point()
    {
        var vertices = new List<Vector3> { new(0, 0, 0) };
        var mesh = new Mesh
        {
            Vertices = vertices,
            Indices = Enumerable.Repeat((uint)0, 1).ToList(),
            Normals = new List<Vector3>(Enumerable.Repeat(Vector3.Zero, vertices.Count)),
            Texture = GetDefaultTexture(vertices)
        };
        ((IMesh)mesh).SetMesh();
        Meshes = new List<IMesh> { mesh };
    }
}