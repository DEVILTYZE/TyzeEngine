using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TyzeEngine.Resources;

namespace TyzeEngine.Interfaces;

public interface IMesh : IDisposable
{
    IReadOnlyList<Vector3> Vertices { get; }
    IReadOnlyList<Vector2> Vertices2D { get; }
    IReadOnlyList<uint> Indices { get; }
    IReadOnlyList<float> TextureCoordinates { get; }
    IReadOnlyList<Vector3> Normals { get; }
    List<Texture> Textures { get; }

    internal void Load();
    internal void Draw(PrimitiveType type);
    internal void SetMesh();
}