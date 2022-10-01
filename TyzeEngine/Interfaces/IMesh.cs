using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Physics;
using TyzeEngine.Resources;

namespace TyzeEngine.Interfaces;

public interface IMesh : IDisposable, IDeepCloneable<IMesh>
{
    INode Node { get; }
    IReadOnlyList<Vector3> Vertices { get; }
    IReadOnlyList<Vector2> Vertices2D { get; }
    IReadOnlyList<Vector3> VerticesTransformed { get; }
    IReadOnlyList<Vector2> VerticesTransformed2D { get; }
    IReadOnlyList<uint> Indices { get; }
    IReadOnlyList<Vector2> TextureCoordinates { get; }
    IReadOnlyList<Vector3> Normals { get; }
    List<Texture> Textures { get; }
    MeshBody MeshBody { get; }
    
    CollisionPoints TestCollision(ITransform transform, IMesh mesh, ITransform bodyTransform);
    internal void Load();
    internal void Draw(Shader shader);
    internal void DrawLines();
    internal void SetMesh(int dimension);
}