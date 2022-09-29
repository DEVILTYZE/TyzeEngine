using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

// ReSharper disable once InconsistentNaming
public static class GJK
{
    private const int GjkEpaMaxIterations = 5;

    public static CollisionPoints TestCollision(MeshBody bodyA, ITransform transformA, MeshBody bodyB, ITransform transformB)
    {
        if (bodyA.Dimension != bodyB.Dimension)
            return CollisionPoints.NonCollides;

        return bodyA.Dimension switch
        {
            2 when TestCollision(bodyA, bodyB, out Simplex<Vector2> simplex2d) => EPA(bodyA, bodyB, simplex2d),
            3 when TestCollision(bodyA, bodyB, out Simplex<Vector3> simplex3d) => EPA(bodyA, bodyB, simplex3d),
            _ => CollisionPoints.NonCollides
        };
    }
    
    private static bool TestCollision(MeshBody bodyA, MeshBody bodyB, out Simplex<Vector2> simplex)
    {
        var supportPoint = GetSupportPoint(bodyA, bodyB, Vector2.UnitX);
        simplex = new Simplex<Vector2>();
        simplex.Add(supportPoint);
        var direction = -supportPoint;

        while (true)
        {
            supportPoint = GetSupportPoint(bodyA, bodyB, direction);

            if (Vector2.Dot(supportPoint, direction) <= 0)
                return false; // Нет коллизии.
            
            simplex.Add(supportPoint);

            if (NextSimplex(simplex, direction))
                return true;
        }
    }
    
    private static bool TestCollision(MeshBody bodyA, MeshBody bodyB, out Simplex<Vector3> simplex)
    {
        var supportPoint = GetSupportPoint(bodyA, bodyB, Vector3.UnitX);
        simplex = new Simplex<Vector3>();
        simplex.Add(supportPoint);
        var direction = -supportPoint;

        while (true)
        {
            supportPoint = GetSupportPoint(bodyA, bodyB, direction);

            if (Vector3.Dot(supportPoint, direction) <= 0)
                return false; // Нет коллизии.
            
            simplex.Add(supportPoint);

            if (NextSimplex(simplex, direction))
                return true;
        }
    }

    private static Vector2 GetSupportPoint(MeshBody bodyA, MeshBody bodyB, Vector2 direction) =>
        bodyA.FindFurthestPoint(direction) - bodyB.FindFurthestPoint(-direction);

    private static Vector3 GetSupportPoint(MeshBody bodyA, MeshBody bodyB, Vector3 direction) =>
        bodyA.FindFurthestPoint(direction) - bodyB.FindFurthestPoint(-direction);

    #region GJK2D

    private static bool NextSimplex(Simplex<Vector2> simplex, Vector2 direction) => simplex.Length switch
    {
        2 => Line(simplex, out direction),
        3 => Triangle(simplex, ref direction),
        _ => false
    };

    private static Vector2 TripleProduct(Vector2 a, Vector2 b, Vector2 c) =>
        Vector3.Cross(Vector3.Cross(new Vector3(a), new Vector3(b)), new Vector3(c)).Xy;

    private static bool Line(Simplex<Vector2> simplex, out Vector2 direction)
    {
        var a = simplex[0];
        var b = simplex[1];
        var ab = b - a;
        var ao = -a;
        direction = TripleProduct(ab, ao, ab);

        return false;
    }

    private static bool Triangle(Simplex<Vector2> simplex, ref Vector2 direction)
    {
        var a = simplex[0];
        var b = simplex[1];
        var c = simplex[2];
        var ab = b - a;
        var ac = c - a;
        var ao = -a;
        var abf = TripleProduct(ac, ab, ab);
        var acf = TripleProduct(ab, ac, ac);
        
        if (SameDirection(abf, ao))
        {
            simplex = new Simplex<Vector2>(new[] { a, b });

            return Line(simplex, out direction);
        }

        if (!SameDirection(acf, ao)) 
            return true;
        
        simplex = new Simplex<Vector2>(new[] { a, c });

        return Line(simplex, out direction);
    }
    
    private static bool SameDirection(Vector2 direction, Vector2 ao) => 
        Vector3.Dot(new Vector3(direction), new Vector3(ao)) > 0;

    #endregion
    
    #region GJK3D

    private static bool NextSimplex(Simplex<Vector3> simplex, Vector3 direction) => simplex.Length switch
    {
        2 => Line(simplex, out direction),
        3 => Triangle(simplex, ref direction),
        4 => Tetrahedron(simplex, ref direction),
        _ => false
    };

    private static bool Line(Simplex<Vector3> simplex, out Vector3 direction)
    {
        var a = simplex[0];
        var b = simplex[1];
        var ab = b - a;
        var ao = -a;

        if (SameDirection(ab, ao))
            direction = Vector3.Cross(Vector3.Cross(ab, ao), ab);
        else
        {
            simplex = new Simplex<Vector3>(new[] { a });
            direction = ao;
        }

        return false;
    }

    private static bool Triangle(Simplex<Vector3> simplex, ref Vector3 direction)
    {
        var a = simplex[0];
        var b = simplex[1];
        var c = simplex[2];
        var ab = b - a;
        var ac = c - a;
        var ao = -a;
        var abc = Vector3.Cross(ab, ac);

        if (SameDirection(Vector3.Cross(abc, ac), ao))
        {
            if (SameDirection(ac, ao))
            {
                simplex = new Simplex<Vector3>(new[] { a, c });
                direction = Vector3.Cross(Vector3.Cross(ac, ao), ac);
            }
            else
                return Line(simplex = new Simplex<Vector3>(new[] { a, b }), out direction);
        }
        else
        {
            if (SameDirection(Vector3.Cross(ab, abc), ao))
                return Line(simplex = new Simplex<Vector3>(new[] { a, b }), out direction);

            if (SameDirection(abc, ao))
                direction = abc;
            else
            {
                simplex = new Simplex<Vector3>(new[] { a, c, b });
                direction = -abc;
            }
        }

        return false;
    }

    private static bool Tetrahedron(Simplex<Vector3> simplex, ref Vector3 direction)
    {
        var a = simplex[0];
        var b = simplex[1];
        var c = simplex[2];
        var d = simplex[3];
        var ab = b - a;
        var ac = c - a;
        var ad = d - a;
        var ao = -a;
        var abc = Vector3.Cross(ab, ac);
        var acd = Vector3.Cross(ac, ad);
        var adb = Vector3.Cross(ad, ab);

        if (SameDirection(abc, ao))
            return Triangle(simplex = new Simplex<Vector3>(new[] { a, b, c }), ref direction);

        if (SameDirection(acd, ao))
            return Triangle(simplex = new Simplex<Vector3>(new[] { a, c, d }), ref direction);

        return !SameDirection(adb, ao) || Triangle(simplex = new Simplex<Vector3>(new[] { a, d, b }), ref direction);
    }

    private static bool SameDirection(Vector3 direction, Vector3 ao) => Vector3.Dot(direction, ao) > 0;

    #endregion

    #region EPA2D

    // ReSharper disable once InconsistentNaming
    private static CollisionPoints EPA(MeshBody bodyA, MeshBody bodyB, Simplex<Vector2> simplex)
    {
        var polytope = simplex.GetList();
        var minNormal = Vector2.Zero;
        var minDistance = float.MaxValue;
        var minIndex = 0;
        var iterations = 0;

        while (Math.Abs(minDistance - float.MaxValue) < float.Epsilon)
        {
            if (iterations++ > GjkEpaMaxIterations)
                break;

            for (var i = 0; i < polytope.Count; ++i)
            {
                var a = polytope[i];
                var b = polytope[(i + 1) % polytope.Count];
                var ab = b - a;
                var normal = Vector2.NormalizeFast(new Vector2(ab.Y, -ab.X));
                var distance = Vector2.Dot(normal, a);

                if (distance < 0)
                {
                    normal *= -1;
                    distance *= -1;
                }
                
                if (distance >= minDistance)
                    continue;

                minNormal = normal;
                minDistance = distance;
                minIndex = i;
            }

            var supportPoint = GetSupportPoint(bodyA, bodyB, minNormal);
            var sDistance = Vector2.Dot(minNormal, supportPoint);
            
            if (MathF.Abs(sDistance - minDistance) <= float.Epsilon)
                continue;

            minDistance = float.MaxValue;
            polytope.Insert(minIndex + 1, supportPoint);
        }
        
        if (Math.Abs(minDistance - float.MaxValue) < float.Epsilon)
            return CollisionPoints.NonCollides;

        return new CollisionPoints
        {
            Normal = new Vector3(minNormal),
            PenetrationDepth = minDistance,
            IsCollides = true
        };
    }

    #endregion
    
    #region EPA3D

    // ReSharper disable once InconsistentNaming
    private static CollisionPoints EPA(MeshBody bodyA, MeshBody bodyB, Simplex<Vector3> simplex)
    { 
        var faces = new List<int>
        {
            0, 1, 2,
            0, 3, 1,
            0, 2, 3,
            1, 3, 2
        };
        var polytope = new List<Vector3>(simplex.GetList());
        var (normals, minFace) = GetFaceNormals(polytope, faces);
        var minNormal = Vector3.Zero;
        var minDistance = float.MaxValue;
        var iterations = 0;

        while (Math.Abs(minDistance - float.MaxValue) < float.Epsilon)
        {
            minNormal = normals[minFace].Xyz;
            minDistance = normals[minFace].W;
            
            if (iterations++ > GjkEpaMaxIterations)
                break;

            var supportPoint = GetSupportPoint(bodyA, bodyB, minNormal);
            var sDistance = Vector3.Dot(minNormal, supportPoint);

            if (MathF.Abs(sDistance - minDistance) <= float.Epsilon) 
                continue;
            
            minDistance = float.MaxValue;
            var uniqueEdges = new List<(int, int)>();

            for (var i = 0; i < normals.Count; ++i)
                if (SameDirection(normals[i].Xyz, supportPoint))
                {
                    var f = i * 3;

                    AddIfUniqueEdge(ref uniqueEdges, faces, f, f + 1);
                    AddIfUniqueEdge(ref uniqueEdges, faces, f + 1, f + 2);
                    AddIfUniqueEdge(ref uniqueEdges, faces, f + 2, f);

                    faces[f + 2] = faces[^1];
                    faces.RemoveAt(faces.Count - 1);
                    faces[f + 1] = faces[^1];
                    faces.RemoveAt(faces.Count - 1);
                    faces[f] = faces[^1];
                    faces.RemoveAt(faces.Count - 1);
                    normals[i] = normals[^1];
                    normals.RemoveAt(normals.Count - 1);
                    --i;
                }

            var newFaces = new List<int>();

            foreach (var (edge1, edge2) in uniqueEdges)
            {
                newFaces.Add(edge1);
                newFaces.Add(edge2);
                newFaces.Add(polytope.Count);
            }
            
            polytope.Add(supportPoint);
            var (newNormals, newMinFace) = GetFaceNormals(polytope, newFaces);
            var oldMinDistance = float.MaxValue;
            
            for (var i = 0; i < normals.Count; ++i)
                if (normals[i].W < oldMinDistance)
                {
                    oldMinDistance = normals[i].W;
                    minFace = i;
                }

            if (newNormals[newMinFace].W < oldMinDistance)
                minFace = newMinFace + normals.Count;

            faces = faces.Concat(newFaces).ToList();
            normals = normals.Concat(newNormals).ToList();
        }

        return new CollisionPoints
        {
            Normal = minNormal,
            PenetrationDepth = minDistance + float.Epsilon,
            IsCollides = true
        };
    }

    private static (List<Vector4>, int) GetFaceNormals(IReadOnlyList<Vector3> polytope, IReadOnlyList<int> faces)
    {
        var normals = new List<Vector4>();
        var minTriangle = 0;
        var minDistance = float.MaxValue;

        for (var i = 0; i < faces.Count; i += 3)
        {
            var a = polytope[faces[i]];
            var b = polytope[faces[i + 1]];
            var c = polytope[faces[i + 2]];
            var normal = Vector3.NormalizeFast(Vector3.Cross(b - a, c - a));
            var distance = Vector3.Dot(normal, a);

            if (distance < 0)
            {
                normal *= -1;
                distance *= -1;
            }
            
            normals.Add(new Vector4(normal, distance));
            
            if (distance >= minDistance)
                continue;

            minDistance = distance;
            minTriangle = i / 3;
        }

        return (normals, minTriangle);
    }

    private static void AddIfUniqueEdge(ref List<(int, int)> edges, IReadOnlyList<int> faces, int a, int b)
    {
        var reverse = edges.Find(edge => edge.Item1 == faces[b] && edge.Item2 == faces[a]);

        if (reverse != edges[^1])
            edges.RemoveAll(edge => edge == reverse);
        else
            edges.Add((faces[b], faces[a]));
    }

    #endregion
}