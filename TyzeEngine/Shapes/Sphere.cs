using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Objects;

namespace TyzeEngine.Shapes;

public class Sphere : Model
{
    public Sphere(float radius = 1, int sectorCount = 32, int stackCount = 32)
    {
        SetModel(radius, sectorCount, stackCount);
        base.SetModel();
    }

    private void SetModel(float radius, int sectorCount, int stackCount)
    {
        var vertices = new List<Vector3>();
        var indices = new List<uint>();
        // var lineIndices = new List<uint>();
        var texture = new VectorArray();
        var normals = new List<Vector3>();

        var sectorStep = MathF.Tau / sectorCount;
        var stackStep = MathF.PI / stackCount;
        var length = 1 / radius;

        for (var i = 0; i <= stackCount; ++i)
        {
            var stackAngle = MathHelper.PiOver2 - i * stackStep;
            var xy = radius * MathF.Cos(stackAngle);
            var z = radius * MathF.Sin(stackAngle);

            for (var j = 0; j <= sectorCount; ++j)
            {
                var sectorAngle = j * sectorStep;
                var x = xy * MathF.Cos(sectorAngle);
                var y = xy * MathF.Sin(sectorAngle);
                vertices.Add(new Vector3(x, y, z));
                normals.Add(new Vector3(x * length, y * length, z * length));
                texture.Add(j / (float)sectorCount, i / (float)stackCount);
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

        Vertices = vertices;
        Normals = normals;
        Texture = texture;
        Indices = indices;
    }
}