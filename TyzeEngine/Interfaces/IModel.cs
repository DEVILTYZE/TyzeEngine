using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace TyzeEngine.Interfaces;

public interface IModel : IDisposable, IGameResource
{
    IReadOnlyList<IMesh> Meshes { get; }
    string Directory { get; }
    string Name { get; }
    bool Loaded { get; }

    void LoadFromFile();
    internal void Load();
    internal void Draw(PrimitiveType type);
}