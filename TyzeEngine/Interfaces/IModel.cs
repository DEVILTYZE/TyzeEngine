using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace TyzeEngine.Interfaces;

public interface IModel : IDisposable
{
    Uid Id { get; }
    IReadOnlyList<Vector3> Vertices { get; }
    string Directory { get; }
    string Name { get; }
    bool Loaded { get; }

    void Load();
    (float[], uint[]) GetVectorArray(IGameObject obj);
    Vector3[] GetScaledVectorArray(Vector3 scale);
}