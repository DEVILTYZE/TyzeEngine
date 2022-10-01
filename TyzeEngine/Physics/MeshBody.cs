using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public class MeshBody : Body
{
    private readonly IMesh _mesh;

    public Matrix4 Transform { get; set; }

    public MeshBody(IMaterial material, IMesh mesh, int dimension) : base(material, dimension)
    {
        _mesh = mesh;
    }

    public Vector2 FindFurthestPoint(Vector2 direction)
    {
        direction.NormalizeFast();
        var maxPoint = Vector2.Zero;
        var maxDistance = float.MinValue;

        foreach (var vertex in _mesh.Vertices2D)
        {
            var transformedVertex = (new Vector4(vertex.X, vertex.Y, 0, 1) * Transform).Xy;
            var distance = Vector2.Dot(transformedVertex, direction);
            
            if (distance <= maxDistance)
                continue;

            maxDistance = distance;
            maxPoint = transformedVertex;
        }

        return maxPoint;
    }

    public Vector3 FindFurthestPoint(Vector3 direction)
    {
        var maxPoint = Vector3.Zero;
        var maxDistance = float.MinValue;

        foreach (var vertex in _mesh.Vertices)
        {
            var transformedVertex = (new Vector4(vertex, 1) * Transform).Xyz;
            var distance = Vector3.Dot(transformedVertex, direction);
            
            if (distance <= maxDistance)
                continue;

            maxDistance = distance;
            maxPoint = transformedVertex;
        }

        return maxPoint;
    }
}