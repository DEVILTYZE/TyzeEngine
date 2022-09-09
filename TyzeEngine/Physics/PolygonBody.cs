using System;
using OpenTK.Mathematics;
using TyzeEngine.Materials;
using TyzeEngine.Objects;

namespace TyzeEngine.Physics;

public class PolygonBody : Body
{
    private readonly Vector2[] _vertices;

    public Vector2[] Vertices
    {
        get => _vertices;
        init
        {
            _vertices = value;
            // RecomputeBodyParameters(Scale);
        }
    }
    public Vector2[] Normals { get; private set; }

    public PolygonBody(IMaterial material) : base(material)
    {
    }

    public Vector2 GetSupportPoint(Vector2 direction)
    {
        var bestProjection = float.MinValue;
        var bestVertex = Vertices[0];

        foreach (var vertex in Vertices)
        {
            var projection = Vector2.Dot(vertex, direction);

            if (projection <= bestProjection)
                continue;

            bestVertex = vertex;
            bestProjection = projection;
        }

        return bestVertex;
    }

    protected override Body DeepClone() => new PolygonBody(Material)
    {
        Vertices = Vertices,
        Normals = Normals
    };
    
    protected sealed override void ComputeMass()
    {
        var centroid = Vector3.Zero;
        var area = .0f;
        var inertia = .0f;
        const float kInv3 = 1 / 3f;

        for (var i = 0; i < Vertices.Length; ++i)
        {
            var i2 = (i + 1) % Vertices.Length;
            var v1 = Vertices[i];
            var v2 = Vertices[i2];
            
            var d = MathF.Abs(Vector2.PerpDot(v1, v2));
            var triangleArea = .5f * d;
            area += triangleArea;
            centroid += new Vector3(triangleArea * (v1 + v2));

            var intX2 = v1.X * v1.X + v2.X * v1.X + v2.X * v2.X;
            var intY2 = v1.Y * v1.Y + v2.Y * v1.Y + v2.Y * v2.Y;
            inertia += .25f * kInv3 * d * (intX2 + intY2);
        }

        Centroid = centroid;
        SetMassAndInertia(Material.Density * area, Material.Density * inertia);
    }

    internal override void RecomputeBodyParameters(Vector3 newScale)
    {
        if (Vertices is null)
            return;

        var scale = new Vector2(newScale.X, newScale.Y);
        
        for(var i = 0; i < Vertices.Length; ++i)
            Vector2.Multiply(Vertices[i], scale, out _vertices[i]);
        
        SetNormals();
        ComputeMass();
    }

    private void SetNormals()
    {
        Normals = new Vector2[Vertices.Length];

        for (var i = 0; i < Normals.Length; ++i)
        {
            var dVector = Vertices[i < Vertices.Length - 1 ? i + 1 : 0] - Vertices[i];
            Normals[i] = new Vector2(dVector.Y, -dVector.X).Normalized();
        }
    }
}