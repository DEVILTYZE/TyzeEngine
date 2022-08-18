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
    
    public static CollisionHandler CollisionEnter { get; set; } = ResolveCollision;
    public static CollisionHandler CollisionStay { get; set; }
    public static CollisionHandler CollisionExit { get; set; }

    public static void DetermineCollision(IReadOnlyList<IGameObject> objects)
    {
        Parallel.For(0, objects.Count - 1, i =>
        {
            for (var j = i + 1; j < objects.Count; ++j)
            {
                if (objects[i].Body.Layer == objects[j].Body.Layer)
                    continue;
                
                var args = objects[i].Body.IsCollisionTo(objects[i].Body, objects[j].Body);

                if (args.IsCollides)
                    CollisionEnter.Invoke(args);
            }
        });
    }

    private static void ResolveCollision(CollisionEventArgs args)
    {
        PositionCorrection(args);
        var (bodyA, bodyB) = (args.BodyA, args.BodyB);
        var velocityA = bodyA.Velocity + bodyA.Force;
        var velocityB = bodyB.Velocity + bodyB.Force;
        var difference = velocityB - velocityA;
        var velocityAlongNormal = Vector3.Dot(difference, args.Normal);

        if (velocityAlongNormal > float.Epsilon)
            return;

        var e = MathF.Min(bodyA.Material.Restitution, bodyB.Material.Restitution);
        var jn = -(1 + e) * velocityAlongNormal / (bodyA.InverseMass + bodyB.InverseMass);
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
            var dynamicFriction = bodyA.Material.DynamicFriction; 
            dynamicFriction = (dynamicFriction + bodyB.Material.DynamicFriction) / 2;
            frictionImpulse = -jn * tangent * dynamicFriction;
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
        var correction = MathF.Max(args.Penetration - slop, .0f) / (bodyA.InverseMass 
            + bodyB.InverseMass) * percent * args.Normal;
        var correction1 = -bodyA.InverseMass * correction;
        var correction2 = bodyB.InverseMass * correction;
        
        lock (CollisionLocker)
        {
            bodyA.Position += correction1;
            bodyB.Position += correction2;
        }
    }

    #region PolygonToPolygon
    
    internal static CollisionEventArgs PolygonToPolygon(IBody bodyA, IBody bodyB)
    {
        var args = new CollisionEventArgs(bodyA, bodyB);
        var polygonBodyA = (PolygonBody)bodyA;
        var polygonBodyB = (PolygonBody)bodyB;

        var distanceA = FindAxisLeastPenetration(out var faceIndexA, polygonBodyA, polygonBodyB);

        if (distanceA >= float.Epsilon)
            return args;
        
        var distanceB = FindAxisLeastPenetration(out var faceIndexB, polygonBodyB, polygonBodyA);
        
        if (distanceB >= float.Epsilon)
            return args;

        args.IsCollides = true;
        var referenceBody = polygonBodyA;
        var incidentBody = polygonBodyB;
        var referencedIndex = faceIndexA;
        var flip = false;
        
        if (!BiasGreaterThan(distanceA, distanceB))
        {
            referenceBody = polygonBodyB;
            incidentBody = polygonBodyA;
            referencedIndex = faceIndexB;
            flip = true;
        }

        var referencedMatrix = referenceBody.RotationMatrix;
        var incidentMatrix = incidentBody.RotationMatrix;

        FindIncidentFace(out var incidentVectors, referenceBody, incidentBody, referencedMatrix, incidentMatrix, 
            referencedIndex);

        var vectors = new Vector3[2];
        vectors[0] = referenceBody.Vertices[referencedIndex];
        referencedIndex = (referencedIndex + 1) % referenceBody.Vertices.Length;
        vectors[1] = referenceBody.Vertices[referencedIndex];
        vectors[0] = referencedMatrix * vectors[0] + referenceBody.Position;
        vectors[1] = referencedMatrix * vectors[1] + referenceBody.Position;
        
        var sidePanelNormal = vectors[1] - vectors[0];
        sidePanelNormal.Normalize();
        var referencedFaceNormal = new Vector3(sidePanelNormal.Y, -sidePanelNormal.X, 0);
        var referencedC = Vector3.Dot(referencedFaceNormal, vectors[1]);
        var negativeSide = -Vector3.Dot(sidePanelNormal, vectors[1]);
        var positiveSide = Vector3.Dot(sidePanelNormal, vectors[1]);

        if (Clip(-sidePanelNormal, negativeSide, ref incidentVectors) < 2)
            return args;
        
        if (Clip(sidePanelNormal, positiveSide, ref incidentVectors) < 2)
            return args;

        args.IsCollides = true;
        args.Normal = flip ? -referencedFaceNormal : referencedFaceNormal;
        var separation = Vector3.Dot(referencedFaceNormal, incidentVectors[0]) - referencedC;

        if (separation <= float.Epsilon)
            args.Penetration = -separation;
        else
            args.Penetration = 0;
        
        separation = Vector3.Dot(referencedFaceNormal, incidentVectors[1]) - referencedC;

        if (separation <= float.Epsilon)
            args.Penetration += -separation;

        return args;
    }

    private static int Clip(Vector3 normal, float c, ref Vector3[] face)
    {
        var sp = 0;
        var outVectors = new[] { face[0], face[1] };
        var d1 = Vector3.Dot(normal, face[0]) - c;
        var d2 = Vector3.Dot(normal, face[1]) - c;

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
            var alpha = d1 / (d1 - d2);
            outVectors[sp] = face[0] + alpha * (face[1] - face[0]);
            ++sp;
        }

        face[0] = outVectors[0];
        face[1] = outVectors[1];

        return sp;
    }

    private static void FindIncidentFace(out Vector3[] vectors, PolygonBody referenceBody, PolygonBody incidentBody, 
        Matrix3 referencedMatrix, Matrix3 incidentMatrix, int referencedIndex)
    {
        var referencedNormal = referenceBody.Normals[referencedIndex];
        referencedNormal = referencedMatrix * referencedNormal;
        Matrix3.Transpose(incidentMatrix, out var transposed);
        referencedNormal = transposed * referencedNormal;
        var incidentFace = 0;
        var minDot = float.MaxValue;

        for (var i = 0; i < incidentBody.Vertices.Length; ++i)
        {
            var dot = Vector3.Dot(referencedNormal, incidentBody.Vertices[i]);

            if (dot >= minDot)
                continue;

            minDot = dot;
            incidentFace = i;
        }

        vectors = new Vector3[2];
        vectors[0] = incidentMatrix * incidentBody.Vertices[incidentFace] + incidentBody.Position;
        incidentFace = (incidentFace + 1) % incidentBody.Vertices.Length;
        vectors[1] = incidentMatrix * incidentBody.Vertices[incidentFace] + incidentBody.Position;
    }

    private static float FindAxisLeastPenetration(out int faceIndex, PolygonBody bodyA, PolygonBody bodyB)
    {
        var bestDistance = float.MinValue;
        var bestIndex = 0;

        for (var i = 0; i < bodyA.Vertices.Length; ++i)
        {
            var normalA = bodyA.Normals[i];
            var support = bodyB.GetSupportPoint(-normalA);
            var vertexA = bodyA.Vertices[i];
            var distance = Vector3.Dot(normalA, support - vertexA);
            
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
        var circleBodyA = (EllipseBody)bodyA;
        var circleBodyB = (EllipseBody)bodyB;
        var normal = circleBodyA.Position - circleBodyA.Position;
        var radius = circleBodyA.Radius + circleBodyB.Radius;
        radius *= radius;
        var distance = normal.LengthSquared;

        if (distance > radius)
            return args;

        args.IsCollides = true;
        distance = MathF.Sqrt(distance);

        if (distance != 0)
        {
            args.Penetration = radius - distance;
            args.Normal = normal;
        }
        else
        {
            args.Penetration = circleBodyA.Radius;
            args.Normal = -normal;
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