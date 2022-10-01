using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

namespace TyzeEngine.Shapes;

public class Point : Model
{
    public Point()
    {
        var vertices = new List<Vector3> { new(0, 0, 0) };
        IMesh mesh = new Mesh(RootNode)
        {
            Vertices = vertices,
            Indices = Enumerable.Repeat((uint)0, 1).ToList(),
            Normals = new List<Vector3>(Enumerable.Repeat(Vector3.Zero, vertices.Count)),
            TextureCoordinates = GetDefaultTexture(vertices)
        };
        mesh.SetMesh(1);
        RootNode.Meshes.Add(mesh);
    }
}