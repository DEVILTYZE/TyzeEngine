﻿using System;
using TyzeEngine.Physics;

namespace TyzeEngine.Interfaces;

public interface IModel : IDisposable, IGameResource, IDeepCloneable<IModel>
{
    INode RootNode { get; }
    string Directory { get; }
    string Name { get; }
    bool Loaded { get; }

    CollisionPoints TestCollision(ITransform transform, IModel model, ITransform bodyTransform);
    void LoadFromFile();
    internal void Load();
    internal void Draw(Shader shader);
    internal void DrawLines();
}