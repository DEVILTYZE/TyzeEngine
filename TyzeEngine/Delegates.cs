using System;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine;

public delegate void TriggerHandler(TriggeredEventArgs args);
public delegate void CollisionHandler(CollisionEventArgs args);

public class TriggeredEventArgs : EventArgs
{
    public object Data { get; }

    public TriggeredEventArgs(object data) => Data = data;
}

public class CollisionEventArgs : EventArgs
{
    public IGameObject ThisObject { get; }
    public IGameObject OtherObject { get; }
    public Vector3 Normal { get; internal set; }
    public float Penetration { get; internal set; }
    public bool IsCollides { get; internal set; }

    public CollisionEventArgs(IGameObject thisObject, IGameObject otherObject)
    {
        ThisObject = thisObject;
        OtherObject = otherObject;
        Normal = Vector3.Zero;
        Penetration = 0;
        IsCollides = false;
    }
}