using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;

namespace TyzeEngine;

internal static class DefaultModels
{
    internal static (Vector3[], uint[]) GetPoint() => (GetPointVertices(), GetPointIndices());

    internal static (Vector3[], uint[]) GetTriangle() => (GetTriangleVertices(), GetTriangleIndices());
    
    internal static (Vector3[], uint[]) GetRectangle() => (GetRectangleVertices(), GetRectangleIndices());

    internal static (Vector3[], uint[]) GetCircle(float angle = (float)Math.PI / 10)
    {
        var vertices = GetCircleVertices(angle);
        var indices = GetCircleIndices(vertices.Length / Constants.Vector3Length - 1);

        return (vertices, indices);
    }
    
    internal static IEnumerable<float> GetDefaultTexture(IReadOnlyList<Vector3> vertices)
    {
        var result = new List<float>(vertices.Count * 2);

        for (var i = 0; i < vertices.Count; ++i)
        {
            result.Add((vertices[i].X + 1) / 2);
            result.Add((vertices[i].Y + 1) / 2);
        }

        return result.ToArray();
    }

    private static Vector3[] GetPointVertices() => new[] { new Vector3(0, 0, 0) };
    
    private static Vector3[] GetTriangleVertices() => 
        new[] { new Vector3(-1f, -1, 0), new Vector3(1, -1, 0), new Vector3(0, 1f, 0) };

    private static Vector3[] GetRectangleVertices() => new[]
    {
        new Vector3(-1f, -1, 0), new Vector3(1, -1, 0), 
        new Vector3(1, 1, 0), new Vector3(-1, 1, 0)
    };
    
    private static Vector3[] GetCircleVertices(float strideAngle)
    {
        var vertices = new List<Vector3>();
        const float radius = 1;
        const float start = 0;

        vertices.Add(new Vector3(start, start, 0));
        
        for (var angle = 0f; angle <= 2 * Math.PI; angle += strideAngle)
        {
            var x = radius * (float)Math.Cos(angle);
            var y = radius * (float)Math.Sin(angle);
            vertices.Add(new Vector3(x, y, 0));
        }

        return vertices.ToArray();
    }

    private static uint[] GetPointIndices() => Enumerable.Repeat((uint)0, 1).ToArray();

    private static uint[] GetTriangleIndices() => new uint[] { 0, 1, 2 };

    private static uint[] GetRectangleIndices() => new uint[] { 0, 1, 2, 0, 2, 3 };

    private static uint[] GetCircleIndices(int count)
    {
        var result = new List<uint>(count);

        for (uint currentPoint = 1; currentPoint < count; ++currentPoint)
        {
            result.Add(0);
            result.Add(currentPoint);
            result.Add(currentPoint + 1);
        }
        
        result.Add(0);
        result.Add((uint)count);
        result.Add(1);

        return result.ToArray();
    }
}