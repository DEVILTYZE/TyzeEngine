using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

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
    
    internal static (Vector3[], uint[]) GetCube() => (GetCubeVertices(), GetCubeIndices());

    internal static (Vector3[], uint[], IVectorArray) GetSphere(in int sectorCount = 32, in int stackCount = 32)
    {
        var vertices = new List<Vector3>();
        var indices = new List<uint>();
        // var lineIndices = new List<uint>();
        var texture = new VectorArray();

        const float radius = 1;
        var sectorStep = 2 * MathF.PI / sectorCount;
        var stackStep = MathF.PI / stackCount;

        for (var i = 0; i < stackCount; ++i)
        {
            var stackAngle = MathF.PI / 2 - i * stackStep;
            var xy = radius * MathF.Cos(stackAngle);
            var z = radius * MathF.Sin(stackAngle);

            for (var j = 0; j < sectorCount; ++j)
            {
                var sectorAngle = j * sectorStep;
                var x = xy * MathF.Cos(sectorAngle);
                var y = xy * MathF.Sin(sectorAngle);
                vertices.Add(new Vector3(x, y, z));
                texture.Add(j * sectorCount, i * stackCount);
            }
        }

        for (var i = 0; i < stackCount; ++i)
        {
            var k1 = (uint)(i * (sectorCount + 1));
            var k2 = (uint)(k1 + sectorCount + 1);

            for (var j = 0; j <= sectorCount; ++j, ++k1, ++k2)
            {
                if (i != 0)
                    indices.AddRange(new[] { k1, k2, k1 + 1 });
                
                if (i != stackCount - 1)
                    indices.AddRange(new[] { k1 + 1, k2, k2 + 1 });
                
                // lineIndices.Add(k1);
                // lineIndices.Add(k2);
                //
                // if (i != 0)
                //     lineIndices.AddRange(new[] { k1, k1 + 1 });
            }
        }

        return (vertices.ToArray(), indices.ToArray(), texture);
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
    
    private static Vector3[] GetTriangleVertices() => new[]
    {
        new Vector3(-1f, -1, 0), 
        new Vector3(1, -1, 0), 
        new Vector3(0, 1f, 0)
    };

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

    private static Vector3[] GetCubeVertices()
    {
        var rect = GetRectangleVertices();
        var backRect = new Vector3[rect.Length];

        for (var i = 0; i < rect.Length; ++i)
            backRect[i] = new Vector3(rect[i].X, rect[i].Y, -2);

        return rect.Concat(backRect).ToArray();
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

    private static uint[] GetCubeIndices() => new uint[]
    {
        0, 1, 2,
        0, 2, 3,
        0, 1, 5,
        0, 5, 4,
        4, 0, 3,
        4, 3, 7,
        1, 5, 6,
        1, 6, 2,
        3, 2, 6,
        3, 6, 7,
        5, 4, 7,
        5, 7, 6
    };
}