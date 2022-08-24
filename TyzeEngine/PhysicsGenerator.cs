using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine;

public static class PhysicsGenerator
{
    private static readonly object CollisionLocker = new();

    public static Dictionary<(Type, Type), Func<IBody, IBody, CollisionEventArgs>> CollisionMethods { get; } = new()
    {
        { (typeof(PolygonBody), typeof(PolygonBody)), PolygonToPolygon },
        { (typeof(PolygonBody), typeof(EllipseBody)), PolygonToCircle },
        { (typeof(EllipseBody), typeof(PolygonBody)), CircleToPolygon },
        { (typeof(EllipseBody), typeof(EllipseBody)), CircleToCircle } 
    };
    public static CollisionHandler CollisionEnter { get; set; } = ResolveCollision;
    public static CollisionHandler CollisionStay { get; set; }
    public static CollisionHandler CollisionExit { get; set; }

    public static void DetermineCollision(IReadOnlyList<IGameObject> objects)
    {
        Parallel.For(0, objects.Count - 1, i =>
        {
            for (var j = i + 1; j < objects.Count; ++j)
            {
                if (objects[i].Body.CollisionLayer == objects[j].Body.CollisionLayer)
                    continue;

                var key = (objects[i].Body.GetType(), objects[j].Body.GetType());
                var args = CollisionMethods[key].Invoke(objects[i].Body, objects[j].Body);

                if (args.IsCollides)
                    CollisionEnter.Invoke(args);
            }
        });
    }
    
    public static Vector3 Cross(in Vector3 vector1, in Vector3 vector2) => new(
        vector1.Y * vector2.Z - vector1.Z * vector2.Y,
        vector1.Z * vector2.X - vector1.X * vector2.Z,
        vector1.X * vector2.Y - vector1.Y * vector2.X
    );

    #region ResolveCollision
    
    private static void ResolveCollision(CollisionEventArgs args)
    {
        PositionCorrection(args);
        var (bodyA, bodyB) = (args.BodyA, args.BodyB);
        var velA = bodyA.Velocity + bodyA.Force;
        var velB = bodyB.Velocity + bodyB.Force;
        var difference = velB - velA;
        var velAlongNormal = Vector3.Dot(difference, args.Normal);

        if (velAlongNormal > float.Epsilon)
            return;

        var e = MathF.Min(bodyA.Material.Restitution, bodyB.Material.Restitution);
        var jn = -(1 + e) * velAlongNormal / (bodyA.InverseMass + bodyB.InverseMass);
        var impulse = args.Normal * jn;
        
        lock (CollisionLocker)
        {
            bodyA.Velocity += -bodyA.InverseMass * impulse;
            bodyB.Velocity += bodyB.InverseMass * impulse;
        }

        difference = bodyB.Velocity - bodyA.Velocity;
        var tangent = difference - Vector3.Dot(difference, args.Normal) * args.Normal;
        tangent.NormalizeFast();

        var orientation1 = MathF.Pow(Vector3.Dot(bodyA.Centroid, tangent), 2) * bodyA.InverseInertia;
        var orientation2 = MathF.Pow(Vector3.Dot(bodyB.Centroid, tangent), 2) * bodyB.InverseInertia;
        var jt = -Vector3.Dot(difference, tangent);
        jt /= bodyA.InverseMass + bodyB.InverseMass + orientation1 + orientation2;

        var mu = (bodyA.Material.StaticFriction + bodyB.Material.StaticFriction) / 2;
        Vector3 frictionImpulse;

        if (MathF.Abs(jt) < jn * mu)
            frictionImpulse = jt * tangent;
        else
        {
            var dynFriction = (bodyA.Material.DynamicFriction + bodyB.Material.DynamicFriction) / 2;
            frictionImpulse = -jn * tangent * dynFriction;
        }
        
        lock (CollisionLocker)
        {
            bodyA.Velocity += -bodyA.InverseMass * frictionImpulse;
            bodyB.Velocity += bodyB.InverseMass * frictionImpulse;
            bodyA.Torque += -bodyA.InverseInertia * frictionImpulse;
            bodyB.Torque += bodyB.InverseInertia * frictionImpulse;
        }
    }

    private static void PositionCorrection(CollisionEventArgs args)
    {
        const float percent = .2f;
        const float slop = .01f;
        var (bodyA, bodyB) = (args.BodyA, args.BodyB);
        var max = MathF.Max(args.Penetration - slop, .0f);
        var correction = max / (bodyA.InverseMass + bodyB.InverseMass) * percent * args.Normal;
        var correction1 = -bodyA.InverseMass * correction;
        var correction2 = bodyB.InverseMass * correction;
        
        lock (CollisionLocker)
        {
            bodyA.Position += correction1;
            bodyB.Position += correction2;
        }
    }
    
    #endregion

    #region PolygonToPolygon
    
    internal static CollisionEventArgs PolygonToPolygon(IBody bodyA, IBody bodyB)
    {
        var args = new CollisionEventArgs(bodyA, bodyB);
        var polyBodyA = (PolygonBody)bodyA;
        var polyBodyB = (PolygonBody)bodyB;

        var distanceA = FindAxisLeastPenetration(out var faceIndexA, polyBodyA, polyBodyB);

        if (distanceA >= 0)
            return args;
        
        var distanceB = FindAxisLeastPenetration(out var faceIndexB, polyBodyB, polyBodyA);
        
        if (distanceB >= 0)
            return args;

        var refBody = polyBodyA;
        var incBody = polyBodyB;
        var refIndex = faceIndexA;
        var flip = false;
        
        if (!BiasGreaterThan(distanceA, distanceB))
        {
            refBody = polyBodyB;
            incBody = polyBodyA;
            refIndex = faceIndexB;
            flip = true;
        }
        
        FindIncidentFace(out var incVectors, refBody, incBody, refIndex);

        var vectors = new Vector2[2];
        vectors[0] = refBody.Vertices[refIndex];
        refIndex = (refIndex + 1) % refBody.Vertices.Length;
        vectors[1] = refBody.Vertices[refIndex];
        vectors[0] = refBody.RotationMatrix * vectors[0] + refBody.Position.Xy;
        vectors[1] = refBody.RotationMatrix * vectors[1] + refBody.Position.Xy;
        
        var sidePanelNormal = vectors[1] - vectors[0];
        sidePanelNormal.NormalizeFast();
        var refFaceNormal = new Vector2(sidePanelNormal.Y, -sidePanelNormal.X);
        var refC = Vector2.Dot(refFaceNormal, vectors[1]);
        var negativeSide = -Vector2.Dot(sidePanelNormal, vectors[1]);
        var positiveSide = Vector2.Dot(sidePanelNormal, vectors[1]);

        if (Clip(-sidePanelNormal, negativeSide, ref incVectors) < 2)
            return args;
        
        if (Clip(sidePanelNormal, positiveSide, ref incVectors) < 2)
            return args;

        args.IsCollides = true;
        args.Normal = flip ? -new Vector3(refFaceNormal) : new Vector3(refFaceNormal);
        var separation = Vector2.Dot(refFaceNormal, incVectors[0]) - refC;

        if (separation <= float.Epsilon)
            args.Penetration = -separation;
        else
            args.Penetration = 0;
        
        separation = Vector2.Dot(refFaceNormal, incVectors[1]) - refC;

        if (separation <= float.Epsilon)
            args.Penetration += -separation;

        return args;
    }

    private static int Clip(Vector2 normal, float c, ref Vector2[] face)
    {
        var sp = 0;
        var outVectors = new[] { face[0], face[1] };
        var d1 = Vector2.Dot(normal, face[0]) - c;
        var d2 = Vector2.Dot(normal, face[1]) - c;

        if (d1 <= float.Epsilon)
        {
            outVectors[sp] = face[0];
            ++sp;
        }

        if (d2 <= float.Epsilon)
        {
            outVectors[sp] = face[1];
            ++sp;
        }

        if (d1 * d2 < float.Epsilon)
        {
            if (sp == 2)
                return 0;
            
            var alpha = d1 / (d1 - d2);
            outVectors[sp] = face[0] + alpha * (face[1] - face[0]);
            ++sp;
        }

        face[0] = outVectors[0];
        face[1] = outVectors[1];

        return sp;
    }

    private static void FindIncidentFace(out Vector2[] vectors, PolygonBody refBody, PolygonBody incBody, int refIndex)
    {
        var refNormal = refBody.Normals[refIndex];
        refNormal = refBody.RotationMatrix * refNormal;
        Matrix2.Transpose(refBody.RotationMatrix, out var transposed);
        refNormal = transposed * refNormal;
        var incFace = 0;
        var minDot = float.MaxValue;

        for (var i = 0; i < incBody.Vertices.Length; ++i)
        {
            var dot = Vector2.Dot(refNormal, incBody.Vertices[i]);

            if (dot >= minDot)
                continue;

            minDot = dot;
            incFace = i;
        }

        vectors = new Vector2[2];
        vectors[0] = incBody.RotationMatrix * incBody.Vertices[incFace] + incBody.Position.Xy;
        incFace = (incFace + 1) % incBody.Vertices.Length;
        vectors[1] = incBody.RotationMatrix * incBody.Vertices[incFace] + incBody.Position.Xy;
    }

    private static float FindAxisLeastPenetration(out int faceIndex, PolygonBody bodyA, PolygonBody bodyB)
    {
        var bestDistance = float.MinValue;
        var bestIndex = 0;

        for (var i = 0; i < bodyA.Vertices.Length; ++i)
        {
            var normal = bodyA.Normals[i];
            Matrix2.Transpose(bodyB.RotationMatrix, out var buT);
            normal = buT * (bodyA.RotationMatrix * normal);
            var support = bodyB.GetSupportPoint(-normal);
            var vertex = buT * (bodyA.RotationMatrix * bodyA.Vertices[i] + bodyA.Position.Xy - bodyB.Position.Xy);
            var distance = Vector2.Dot(normal, support - vertex);
            
            if (distance <= bestDistance)
                continue;

            bestDistance = distance;
            bestIndex = i;
        }

        faceIndex = bestIndex;

        return bestDistance;
    }

    private static bool BiasGreaterThan(float number1, float number2) => number1 >= number2 * .95f + number1 * .01f;
    
    #endregion
    
    #region CircleToCircle

    internal static CollisionEventArgs CircleToCircle(IBody bodyA, IBody bodyB)
    {
        var args = new CollisionEventArgs(bodyA, bodyB);
        var cirBodyA = (EllipseBody)bodyA;
        var cirBodyB = (EllipseBody)bodyB;
        var n = cirBodyA.Position - cirBodyA.Position;
        var r = cirBodyA.Radius + cirBodyB.Radius;
        r *= r;
        var distance = n.LengthSquared;

        if (distance > r)
            return args;

        args.IsCollides = true;
        distance = MathF.Sqrt(distance);

        if (distance != 0)
        {
            args.Penetration = r - distance;
            args.Normal = n;
        }
        else
        {
            args.Penetration = cirBodyA.Radius;
            args.Normal = -n;
        }

        return args;
    }
    
    #endregion
    
    #region PolygonToCircle
    
    internal static CollisionEventArgs CircleToPolygon(IBody circle, IBody rectangle)
    {
        throw new NotImplementedException();
    }
    
    internal static CollisionEventArgs PolygonToCircle(IBody rectangle, IBody circle)
    {
        throw new NotImplementedException();
    }

    #endregion
}
