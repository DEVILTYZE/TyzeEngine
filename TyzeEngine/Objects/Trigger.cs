﻿using System;
using System.Linq;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public delegate void TriggerHandler(TriggeredEventArgs args);

public class Trigger : ITrigger, ISaveable
{
    private readonly int _placeId;

    protected event TriggerHandler Triggered;
    
    public int Id { get; }
    public bool IsTriggered { get; set; }
    public bool SaveStatus { get; }

    private Trigger(int id, bool notSave)
    {
        Id = id;
        SaveStatus = !notSave;
    }

    public Trigger(int id, IScene scene, int placeId, bool notSave = false) : this(id, notSave)
    {
        _placeId = placeId;
        Triggered += scene.LoadPlace;
    }

    public Trigger(int id, IScript script, bool notSave = false) : this(id, notSave)
    {
        _placeId = -1;
        Triggered += script.Execute;
    }

    public byte[] GetSaveData()
    {
        var id = BitConverter.GetBytes(Id);
        var isTriggered = BitConverter.GetBytes(IsTriggered);

        return id.Concat(isTriggered).ToArray();
    }

    protected virtual void OnTriggered()
    {
        Triggered?.Invoke(new TriggeredEventArgs(_placeId));
        IsTriggered = true;
    }
}