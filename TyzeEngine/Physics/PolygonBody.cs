using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine.Physics;

public class PolygonBody : Body
{
    public Vector3[] Vertices { get; private set; }
    public Vector3[] Normals { get; private set; }

    public PolygonBody(IMaterial material) 
        : base(material)
    {
        AddMethod(typeof(PolygonBody), PhysicsGenerator.PolygonToPolygon);
        AddMethod(typeof(EllipseBody), PhysicsGenerator.PolygonToCircle);
    }
    
    public override CollisionEventArgs IsCollisionTo(IBody bodyA, IBody bodyB) 
        => CollisionMethods[bodyB.GetType()].Invoke(bodyA, bodyB);

    public void SetVertices(IEnumerable<Vector3> vertices)
    {
        Vertices = vertices.ToArray();
        SetNormals();
        ComputeMass();
    }

    protected override Body CloneBody()
        => new PolygonBody(Material)
        {
            Vertices = Vertices,
            Normals = Normals
        };
    

    protected sealed override void ComputeMass()
    {
        Vector3 Cross(Vector3 vector1, Vector3 vector2)
        {
            var left = new System.Numerics.Vector3(vector1.X, vector1.Y, vector1.Z);
            var right = new System.Numerics.Vector3(vector2.X, vector2.Y, vector2.Z);
            var result = System.Numerics.Vector3.Cross(left, right);
            
            return new Vector3(result.X, result.Y, result.Z);
        }
        
        var centroid = Vector3.Zero;
        var area = .0f;
        var inertia = .0f;
        const float kInv3 = 1 / 3f;

        for (var i = 0; i < Vertices.Length; ++i)
        {
            var i2 = (i + 1) % Vertices.Length;
            var vertex1 = Vertices[i];
            var vertex2 = Vertices[i2];
            
            var d = MathF.Abs(Cross(vertex1, vertex2).Length);
            var triangleArea = .5f * d;
            area += triangleArea;
            centroid += triangleArea * (vertex1 + vertex2);

            var intX2 = vertex1.X * vertex1.X + vertex2.X * vertex1.X + vertex2.X * vertex2.X;
            var intY2 = vertex1.Y * vertex1.Y + vertex2.Y * vertex1.Y + vertex2.Y * vertex2.Y;
            inertia += .25f * kInv3 * d * (intX2 + intY2);
        }

        Centroid = centroid;
        SetMassAndInertia(Material.Density * area, Material.Density * inertia);
    }

    private void SetNormals()
    {
        Normals = new Vector3[Vertices.Length - 1];

        for (var i = 0; i < Vertices.Length - 1; ++i)
        {
            var dVector = Vertices[i + 1] - Vertices[i];
            Normals[i] = new Vector3(dVector.Y, -dVector.X, 0).Normalized();
        }
    }

    public Vector3 GetSupportPoint(Vector3 direction)
    {
        var bestProjection = float.MinValue;
        var bestVertex = Vertices[0];

        foreach (var vertex in Vertices)
        {
            var projection = Vector3.Dot(vertex, direction);

            if (projection <= bestProjection)
                continue;

            bestVertex = vertex;
            bestProjection = projection;
        }

        return bestVertex;
    }
}