﻿using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Resources;

namespace TyzeEngine.Shapes;

public class Triangle : Model
{
    public Triangle(float size = 1)
    {
        var vertices = new List<Vector3>
        {
            new(-size, -size, 0), new(size, -size, 0), new(0, size, 0)
        };
        IMesh mesh = new Mesh(RootNode)
        {
            Vertices = vertices,
            Indices = new List<uint> { 0, 1, 2 },
            Normals = Enumerable.Repeat(-Vector3.UnitZ, vertices.Count).ToList(),
            TextureCoordinates = GetDefaultTexture(vertices)
        };
        mesh.SetMesh(2);
        RootNode.Meshes.Add(mesh);
    }
}