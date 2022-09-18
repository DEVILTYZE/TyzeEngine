using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine.Shapes;

public class Circle : Model
{
    public Circle(float radius = 1, float angle = MathF.PI / 20)
    {
        var vertices = GetVertices(radius, angle);
        var indices = GetIndices(vertices.Count - 1);
        var normals = new List<Vector3>(vertices);
        var texture = GetDefaultTexture(vertices);
        
        var mesh = new Mesh
        {
            Vertices = vertices,
            Indices = indices,
            Normals = normals,
            Texture = texture,
        };
        ((IMesh)mesh).SetMesh();
        Meshes = new List<IMesh> { mesh };
    }

    private static List<Vector3> GetVertices(float radius, float angle)
    {
        var vertices = new List<Vector3>();
        const float start = 0;

        vertices.Add(new Vector3(start, start, 0));
        
        for (var localAngle = 0f; localAngle <= Math.Tau; localAngle += angle)
        {
            var x = radius * (float)Math.Cos(localAngle);
            var y = radius * (float)Math.Sin(localAngle);
            vertices.Add(new Vector3(x, y, 0));
        }

        return vertices;
    }
    
    private static List<uint> GetIndices(int verticesCount)
    {
        var result = new List<uint>(verticesCount);

        for (uint currentPoint = 1; currentPoint < verticesCount; ++currentPoint)
        {
            result.Add(0);
            result.Add(currentPoint);
            result.Add(currentPoint + 1);
        }
        
        result.Add(0);
        result.Add((uint)verticesCount);
        result.Add(1);

        return result;
    }
}