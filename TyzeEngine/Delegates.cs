﻿using System;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

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
    public IBody BodyA { get; }
    public IBody BodyB { get; }
    public Vector3 Normal { get; set; }
    public float Penetration { get; set; }
    public bool IsCollides { get; set; }
    public Vector3[] Contacts { get; }
    public int ContactsCount { get; set; }

    public CollisionEventArgs(IBody bodyA, IBody bodyB)
    {
        BodyA = bodyA;
        BodyB = bodyB;
        Normal = Vector3.Zero;
        Penetration = 0;
        IsCollides = false;
        Contacts = new Vector3[2];
        ContactsCount = 0;
    }
}