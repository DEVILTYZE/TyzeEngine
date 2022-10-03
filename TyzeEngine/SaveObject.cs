using System;
using System.Linq;
using System.Text.Json;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine;

[Serializable]
public struct SaveObject : ISaveable
{
    private int _lastSpaceId = 0;
    private int _lastObjectId = 0;
    
    public UID[] SpaceIds { get; set; }
    public UID[][] SpaceObjects { get; set; }
    public SaveObjectState[] ObjectStates { get; set; }

    public SaveObject(int spacesCount, int objectsCount)
    {
        SpaceIds = new UID[spacesCount];
        SpaceObjects = new UID[spacesCount][];
        ObjectStates = new SaveObjectState[objectsCount];
    }

    public void AddSpaceObjects(ISpace space)
    {
        SpaceIds[_lastSpaceId] = space.Id;
        SpaceObjects[_lastSpaceId] = space.GameObjects.Select(obj => obj.Id).ToArray();
        ++_lastSpaceId;
    }

    public void AddSaveObjectState(IGameObject obj)
    {
        ObjectStates[_lastObjectId] = new SaveObjectState(obj);
        ++_lastObjectId;
    }

    public byte[] Save() => JsonSerializer.SerializeToUtf8Bytes(this);

    public static SaveObject GetByBytes(byte[] data) => JsonSerializer.Deserialize<SaveObject>(data);
}

[Serializable]
public struct SaveObjectState : ISaveable
{
    public UID Id { get; set; }
    public UID ModelId { get; set; }
    public UID ResourceId { get; set; }
    public string BodyTypeName { get; set; }
    public byte[] Position { get; set; }
    public byte[] Scale { get; set; }
    public byte[] Rotation { get; set; }
    public byte[] Color { get; set; }
    public int VisibilityType { get; set; }
    public int Visual { get; set; }
    public int CollisionLayer { get; set; }
    public byte[] Material { get; set; }
    public byte[] Force { get; set; }
    public byte[] GravityForce { get; set; }
    
    public SaveObjectState(IGameObject obj)
    {
        Id = obj.Id;
        ModelId = obj.Model.Id;
        ResourceId = UID.Default;
        BodyTypeName = obj.Body.GetType().FullName;
        Position = Vector.ToBytes(obj.Transform.Position);
        Scale = Vector.ToBytes(obj.Transform.Scale);
        Rotation = Vector.ToBytes(obj.Transform.Rotation);
        Color = Vector.ToBytes(Color4.ToXyz(obj.Color));
        VisibilityType = (int)obj.Visibility;
        Visual = (int)obj.VisualType;
        CollisionLayer = obj.Body.CollisionLayer;
        Material = MaterialToBytes(obj.Body.Material);
        Force = Vector.ToBytes(obj.Body.Force);
        GravityForce = Vector.ToBytes(obj.Body.GravityForce);
    }

    public byte[] Save() => JsonSerializer.SerializeToUtf8Bytes(this);

    public static SaveObjectState GetByBytes(byte[] data) => JsonSerializer.Deserialize<SaveObjectState>(data);

    private static byte[] MaterialToBytes(IMaterial material)
    {
        var density = material.Density;
        var restitution = material.Restitution;
        var staticFriction = material.StaticFriction;
        var dynamicFriction = material.DynamicFriction;

        return Vector.GetBytes(new[] { density, restitution, staticFriction, dynamicFriction }, 4);
    }
}