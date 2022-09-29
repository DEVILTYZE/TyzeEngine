using System;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine;

public delegate void TriggerHandler();
public delegate void CollisionHandler(CollisionEventArgs args);

public class TriggeredEventArgs : EventArgs
{
    public UId ObjectId { get; }
    
    public static readonly TriggeredEventArgs NullArgs = new(UId.Default);

    public TriggeredEventArgs(UId id)
    {
        ObjectId = id;
    }
}

public class CollisionEventArgs : EventArgs
{
    public IGameObject ObjectA { get; }
    public IGameObject ObjectB { get; }
    public CollisionPoints Points { get; }

    public CollisionEventArgs(IGameObject objectA, IGameObject objectB, CollisionPoints points)
    {
        ObjectA = objectA;
        ObjectB = objectB;
        Points = points;
    }
}