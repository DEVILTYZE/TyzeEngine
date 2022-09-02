using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace TyzeEngine.Interfaces;

public interface IModel : IDisposable, IUIdObject
{
    IReadOnlyList<Vector3> Vertices { get; }
    IEnumerable<Vector2> Vertices2D { get; }
    string Directory { get; }
    string Name { get; }
    bool Loaded { get; }

    void Load();
    (float[], uint[]) GetVectorArray(IGameObject obj);
}