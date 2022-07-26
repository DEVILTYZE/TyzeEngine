using System;
using System.Collections.Generic;
using System.Linq;

namespace TyzeEngine;

public static class DefaultModels
{
    public static (float[], uint[]) GetPoint() => (GetPointVertices(), GetPointIndices());

    public static (float[], uint[]) GetTriangle() => (GetTriangleVertices(), GetTriangleIndices());
    
    public static (float[], uint[]) GetRectangle() => (GetRectangleVertices(), GetRectangleIndices());

    public static (float[], uint[]) GetCircle(float angle = (float)Math.PI / 10)
    {
        var vertices = GetCircleVertices(angle);
        var indices = GetCircleIndices(vertices.Length / Constants.VertexLength - 1);

        return (vertices, indices);
    }
    
    internal static IEnumerable<float> GetDefaultTexture(IReadOnlyList<float> points)
    {
        var result = new List<float>(points.Count * 2 / 3);

        for (var i = 0; i < points.Count; i += 3)
        {
            result.Add((points[i] + 1) / 2);
            result.Add((points[i + 1] + 1) / 2);
        }

        return result.ToArray();
    }

    private static float[] GetPointVertices() => Enumerable.Repeat(0f, 3).ToArray();
    
    private static float[] GetTriangleVertices() => new[] { -1f, -1, 0, 1, -1, 0, 0, 1f, 0 };

    private static float[] GetRectangleVertices() => new[] { -1f, -1, 0, 1, -1, 0, 1, 1, 0, -1, 1, 0 };
    
    private static float[] GetCircleVertices(float strideAngle)
    {
        var points = new List<float>();
        const float radius = 1;
        const float start = 0;

        points.AddRange(new[] { start, start, 0 });
        
        for (var angle = 0f; angle <= 2 * Math.PI; angle += strideAngle)
        {
            var x = radius * (float)Math.Cos(angle);
            var y = radius * (float)Math.Sin(angle);
            points.AddRange(new[] { x, y, 0 });
        }

        return points.ToArray();
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