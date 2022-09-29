using OpenTK.Mathematics;
using TyzeEngine.Resources;

namespace TyzeEngine.Physics;

public class MeshBody : Body
{
    private readonly IMesh _mesh;

    public MeshBody(IMaterial material, IMesh mesh, int dimension) : base(material, dimension)
    {
        _mesh = mesh;
    }

    public Vector2 FindFurthestPoint(Vector2 direction)
    {
        var maxPoint = Vector2.Zero;
        var maxDistance = float.MinValue;

        foreach (var vertex in _mesh.Vertices2D)
        {
            var distance = Vector2.Dot(vertex, direction);
            
            if (distance <= maxDistance)
                continue;

            maxDistance = distance;
            maxPoint = vertex;
        }

        return maxPoint;
    }

    public Vector3 FindFurthestPoint(Vector3 direction)
    {
        var maxPoint = Vector3.Zero;
        var maxDistance = float.MinValue;

        foreach (var vertex in _mesh.Vertices)
        {
            var distance = Vector3.Dot(vertex, direction);
            
            if (distance <= maxDistance)
                continue;

            maxDistance = distance;
            maxPoint = vertex;
        }

        return maxPoint;
    }
}