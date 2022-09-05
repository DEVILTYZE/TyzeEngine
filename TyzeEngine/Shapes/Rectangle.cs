using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine.Shapes;

public class Rectangle : Model
{
    public Rectangle(float size = 1) : base(GetModel(size))
    {
    }
    
    private static (List<Vector3>, uint[], IVectorArray) GetModel(float size)
    {
        var vertices = new List<Vector3> 
        { 
            new(-size, -size, 0), new(size, -size, 0), 
            new(size, size, 0), new(-size, size, 0) 
        };
        var indices = new uint[] { 0, 1, 2, 0, 2, 3 };

        return (vertices, indices, GetDefaultTexture(vertices));
    }
}