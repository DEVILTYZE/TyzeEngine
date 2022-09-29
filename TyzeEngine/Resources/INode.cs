using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine.Resources;

public interface INode : IDeepCloneable<INode>, IDisposable
{
    INode Parent { get; }
    List<INode> Children { get; } 
    List<IMesh> Meshes { get; }
    Matrix4 Transform { get; }
    bool HasChildren { get; }
    bool HasMeshes { get; }
    bool IsRoot { get; }

    CollisionPoints TestCollision(ITransform transform, INode node, ITransform bodyTransform);
    internal void Load();
    internal void Draw(Shader shader);
    internal void DrawLines();
}