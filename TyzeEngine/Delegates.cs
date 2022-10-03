using System;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine;

public delegate void TriggerHandler();
public delegate void CollisionHandler(Manifold manifold);

public class TriggeredEventArgs : EventArgs
{
    public UID ObjectId { get; }
    
    public static readonly TriggeredEventArgs NullArgs = new(UID.Default);

    public TriggeredEventArgs(UID id)
    {
        ObjectId = id;
    }
}

public class Manifold : EventArgs
{
    public IGameObject ObjectA { get; }
    public IGameObject ObjectB { get; }
    public CollisionPoints Points { get; }

    public Manifold(IGameObject objectA, IGameObject objectB, CollisionPoints points)
    {
        ObjectA = objectA;
        ObjectB = objectB;
        Points = points;
    }
}