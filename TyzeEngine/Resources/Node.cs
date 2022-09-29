using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine.Resources;

public class Node : INode
{
    private bool _disposed;
    
    public INode Parent { get; }
    public List<INode> Children { get; private set; } = new();
    public List<IMesh> Meshes { get; private set; } = new();
    public Matrix4 Transform => IsRoot ? Parent.Transform : Matrix4.Identity;
    public bool HasChildren => Children.Count > 0;
    public bool HasMeshes => Meshes.Count > 0;
    public bool IsRoot { get; }

    public Node(INode parent)
    {
        Parent = parent;
        IsRoot = Parent is null;
    }

    ~Node() => Dispose(false);

    public INode Clone(INode obj = null)
    {
        var currentNode = (Node)obj ?? new Node(Parent?.Clone());
        currentNode.Children = Children.Select(node => node.Clone()).ToList();
        currentNode.Meshes = Meshes.Select(mesh => mesh.Clone()).ToList();

        return currentNode;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public CollisionPoints TestCollision(ITransform transform, INode node, ITransform bodyTransform)
    {
        var points = CollisionPoints.NonCollides;

        for (var i = 0; i < Meshes.Count && !points.IsCollides; ++i)
        for (var j = 0; j < node.Meshes.Count && !points.IsCollides; ++j)
            points = Meshes[i].TestCollision(transform, node.Meshes[j], bodyTransform);

        if (points.IsCollides)
            return points;

        for (var i = 0; i < Children.Count && !points.IsCollides; ++i)
        for (var j = 0; j < node.Children.Count && !points.IsCollides; ++j)
            points = Children[i].TestCollision(transform, node.Children[j], bodyTransform);

        return points;
    }

    void INode.Load()
    {
        Children.ForEach(node => node.Load());
        Meshes.ForEach(mesh => mesh.Load());
    }

    void INode.Draw(Shader shader)
    {
        Children.ForEach(node => node.Draw(shader));
        Meshes.ForEach(mesh => mesh.Draw(shader));
    }

    void INode.DrawLines()
    {
        Children.ForEach(node => node.DrawLines());
        Meshes.ForEach(mesh => mesh.DrawLines());
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Children.ForEach(node => node.Dispose());
            Meshes.ForEach(mesh => mesh.Dispose());
        }
        
        _disposed = true;
    }
}