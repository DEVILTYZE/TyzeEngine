using System;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine;

public delegate void TriggerHandler();
public delegate void CollisionHandler(CollisionEventArgs args);

public class TriggeredEventArgs : EventArgs
{
    public object Data { get; }
    public static readonly TriggeredEventArgs NullArgs = new(null);

    public TriggeredEventArgs(object data) => Data = data;
}

public class CollisionEventArgs : EventArgs
{
    public IGameObject ObjectA { get; }
    public IGameObject ObjectB { get; }
    public Vector3 Normal { get; set; }
    public float Penetration { get; set; }
    public bool IsCollides { get; set; }

    public CollisionEventArgs(IGameObject objectA, IGameObject objectB)
    {
        ObjectA = objectA;
        ObjectB = objectB;
        Normal = Vector3.Zero;
        Penetration = 0;
        IsCollides = false;
    }
}