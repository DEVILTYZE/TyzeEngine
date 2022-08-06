using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine.Physics;

public class RectangleBody : Body
{
    public Vector3[] Vertices { get; } = new Vector3[Constants.RectanglePointsCount];
    public Vector3[] Normals { get; } = new Vector3[Constants.RectanglePointsCount];
    public float MaxX { get; private set; }
    public float MaxY { get; private set; }
    public float MinX { get; private set; }
    public float MinY { get; private set; }

    public RectangleBody() : base(Material.Static, 0, 0, false)
    {
    }
    
    public RectangleBody(IMaterial material, float volume, float inertia, bool isEnabled, 
        IReadOnlyList<Vector3> points) 
        : base(material, volume, inertia, isEnabled)
    {
        for (var i = 0; i < Vertices.Length; ++i)
            Vertices[i] = points[i];
        
        SetNormals();
        CreateNewRectangle();
        AddMethod(typeof(RectangleBody), PhysicsGenerator.RectangleWithRectangle);
    }
    
    public override CollisionEventArgs IsCollisionWith(IBody bodyA, IBody bodyB) 
        => CollisionMethods[bodyB.GetType()].Invoke(bodyA, bodyB);

    protected override IBody CloneThis() 
        => new RectangleBody(ObjectMaterial, Volume, Inertia, IsEnabled, Vertices);

    private void SetNormals()
    {
        Normals[0] = new Vector3(0, -1, 0);
        Normals[1] = new Vector3(1, 0, 0);
        Normals[2] = new Vector3(0, 1, 0);
        Normals[3] = new Vector3(-1, 0, 0);
    }
    
    private void CreateNewRectangle()
    {
        // if (_vertices[0] + _vertices[2] != _vertices[1] + _vertices[3])
        //     (_vertices[2], _vertices[1]) = (_vertices[1], _vertices[2]);
        //
        // if (_vertices[0] + _vertices[2] != _vertices[1] + _vertices[3])
        //     (_vertices[0], _vertices[1]) = (_vertices[1], _vertices[0]);
        
        MaxX = MathF.Max(MathF.Max(Vertices[0].X, Vertices[1].X), MathF.Max(Vertices[2].X, Vertices[3].X));
        MaxY = MathF.Max(MathF.Max(Vertices[0].Y, Vertices[1].Y), MathF.Max(Vertices[2].Y, Vertices[3].Y));
        MinX = MathF.Min(MathF.Min(Vertices[0].X, Vertices[1].X), MathF.Min(Vertices[2].X, Vertices[3].X));
        MinY = MathF.Min(MathF.Min(Vertices[0].Y, Vertices[1].Y), MathF.Min(Vertices[2].Y, Vertices[3].Y));
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