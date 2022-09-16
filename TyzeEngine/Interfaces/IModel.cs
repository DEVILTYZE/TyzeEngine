﻿using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace TyzeEngine.Interfaces;

public interface IModel : IDisposable, IGameResource
{
    internal ArrayObject ArrayObject { get; set; }
    
    IReadOnlyList<Vector3> Vertices { get; }
    IEnumerable<Vector2> Vertices2D { get; }
    IReadOnlyList<uint> Indices { get; }
    IEnumerable<float> TextureCoordinates { get; }
    IReadOnlyList<Vector3> Normals { get; }
    string Directory { get; }
    string Name { get; }
    bool Loaded { get; }

    void LoadFromFile();
    internal void Load();
}