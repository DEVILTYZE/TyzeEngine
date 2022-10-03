using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics.Bodies;

public class RectangleBody : Body
{
    public Vector2[] Vertices { get; private set; }
    public Vector2 Max => Vertices[2];
    public Vector2 Min => Vertices[0];

    public RectangleBody(IMaterial material, IMesh mesh, bool zeroMass) : base(material, 2)
    {
        if (mesh.Vertices.Count != 4)
            throw new ArgumentException("Vertices count doesn't equals to 4.");

        var list = mesh.Vertices2D.ToList();
        SetVertices(list);
        
        if (!zeroMass)
        {
            var area = CalculateArea(list);
            SetMassAndInertia(area, area);
        }
        else SetMassAndInertia(0, 0);
    }

    public RectangleBody(IMaterial material, IReadOnlyCollection<Vector2> vertices, bool zeroMass) : base(material, 2)
    {
        if (vertices.Count != 4)
            throw new ArgumentException("Vertices count doesn't equals to 4.");

        var list = vertices.ToList();
        SetVertices(list);
        
        if (!zeroMass)
        {
            var area = CalculateArea(list);
            SetMassAndInertia(area, area);
        }
        else SetMassAndInertia(0, 0);
    }

    public override IBody Clone(IBody obj = null) => 
        (RectangleBody)base.Clone(obj ?? new RectangleBody(Material, Vertices, false));

    private void SetVertices(List<Vector2> vertices)
    {
        float maxX = float.MinValue, minX = float.MaxValue, maxY = float.MinValue, minY = float.MaxValue;
        
        vertices.ForEach(vertex =>
        {
            if (vertex.X > maxX)
                maxX = vertex.X;
            if (vertex.X < minX)
                minX = vertex.X;
            if (vertex.Y > maxY)
                maxY = vertex.Y;
            if (vertex.Y < minY)
                minY = vertex.Y;
        });

        Vertices = new Vector2[] { new(minX, minY), new(maxX, minY), new(maxX, maxY), new(minX, maxY) };
    }

    private float CalculateArea(IReadOnlyList<Vector2> vertices)
    {
        var area = 0f;

        for (var i = 0; i < vertices.Count; ++i)
        {
            var i2 = i == Vertices.Length - 1 ? 0 : i + 1;
            area += vertices[i].X * vertices[i2].Y;
        }

        for (var i = 0; i < vertices.Count; ++i)
        {
            var i2 = i == Vertices.Length - 1 ? 0 : i + 1;
            area += -(vertices[i].Y * vertices[i2].X);
        }

        return area * .5f;
    }
}