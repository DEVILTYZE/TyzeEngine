using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Resources;

namespace TyzeEngine.Shapes;

public class Cube : Model
{
    public Cube(float size = 1)
    {
        IMesh mesh = new Mesh(RootNode)
        {
            Vertices = new List<Vector3>
            {
                new(size, size, -size),   new(-size, size, -size), new(-size, size, size), 
                new(size, size, size),    new(size, -size, size),  new(size, size, size), 
                new(-size, size, size),   new(-size, -size, size), new(-size, -size, size), 
                new(-size, size, size),   new(-size, size, -size), new(-size, -size, -size),
                new(-size, -size, -size), new(size, -size, -size), new(size, -size, size), 
                new(-size, -size, size),  new(size, -size, -size), new(size, size, -size), 
                new(size, size, size),    new(size, -size, size),  new(-size, -size, -size), 
                new(-size, size, -size),  new(size, size, -size),  new(size, -size, -size)
            },
            Indices = new List<uint>
            {
                0, 1, 2, 0, 2, 3, 4, 5, 6, 4, 6, 7, 
                8, 9, 10, 8, 10, 11, 12, 13, 14, 12, 14, 15, 
                16, 17, 18, 16, 18, 19, 20, 21, 22, 20, 22, 23
            },
            TextureCoordinates = new List<Vector2>
            {
                new(.625f, .50f), new(.875f, .50f), new(.875f, .25f), new(.625f, .25f), 
                new(.375f, .25f), new(.625f, .25f), new(.625f, 0.0f), new(.375f, 0.0f), 
                new(.375f, 1.0f), new(.625f, 1.0f), new(.625f, .75f), new(.375f, .75f), 
                new(.125f, .50f), new(.375f, .50f), new(.375f, .25f), new(.125f, .25f), 
                new(.375f, .50f), new(.625f, .50f), new(.625f, .25f), new(.375f, .25f),
                new(.375f, .75f), new(.625f, .75f), new(.625f, .50f), new(.375f, .50f)
            },
            Normals = Enumerable.Repeat(Vector3.UnitY, 4)
                .Concat(Enumerable.Repeat(Vector3.UnitZ, 4))
                .Concat(Enumerable.Repeat(-Vector3.UnitX, 4))
                .Concat(Enumerable.Repeat(-Vector3.UnitY, 4))
                .Concat(Enumerable.Repeat(Vector3.UnitX, 4))
                .Concat(Enumerable.Repeat(-Vector3.UnitZ, 4)).ToList()
        };
        mesh.SetMesh(3);
        RootNode.Meshes.Add(mesh);
    }
}