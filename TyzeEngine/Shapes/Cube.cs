using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine.Shapes;

public class Cube : Model
{
    public Cube(float size = 1)
    {
        var vertices = GetVertices(size);
        var mesh = new Mesh
        {
            Vertices = vertices,
            Indices = new List<uint>
            {
                4, 2, 0, 2, 7, 3, 6, 5, 7, 1, 7, 5,
                0, 3, 1, 4, 1, 5, 4, 6, 2, 2, 6, 7,
                6, 4, 5, 1, 3, 7, 0, 2, 3, 4, 0, 1
            },
            Texture = new VectorArray(new[]
            {
                .875f, .5f, .625f, .75f, .625f, .5f, .625f, .75f, .375f, 1, .375f, .75f, .625f, 0, .375f, .25f, .375f,
                0,
                .375f, .5f, .125f, .75f, .125f, .5f, .625f, .5f, .375f, .75f, .375f, .5f, .625f, .25f, .375f, .5f,
                .375f,
                .25f, .875f, .5f, .875f, .75f, .625f, .75f, .625f, .75f, .625f, 1, .375f, 1, .625f, 0, .625f, .25f,
                .375f,
                .25f, .375f, .5f, .375f, .75f, .125f, .75f, .625f, .5f, .625f, .75f, .375f, .75f, .625f, .25f, .625f,
                .5f,
                .375f, .5f
            }, ArrayType.TwoDimensions),
            Normals = vertices.Select(Vector3.NormalizeFast).ToList()
        };
        ((IMesh)mesh).SetMesh();
        Meshes = new List<IMesh> { mesh };
    }
    
    private static List<Vector3> GetVertices(float size)
    {
        var rect = new Vector3[]
        {
            new(size, size, size), new(size, size, -size), 
            new(size, -size, size), new(size, -size, -size)
        };
        var backRect = new Vector3[rect.Length];

        for (var i = 0; i < rect.Length; ++i)
            backRect[i] = new Vector3(-rect[i].X, rect[i].Y, rect[i].Z);

        return rect.Concat(backRect).ToList();
    }
}