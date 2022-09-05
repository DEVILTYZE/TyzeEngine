using System;
using System.Linq;
using System.Text.Json;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine.Objects;

[Serializable]
public struct SaveObject : ISaveable
{
    private int _lastPlaceId = 0;
    private int _lastObjectId = 0;
    
    public int CurrentSceneIndex { get; set; }
    public UId[] PlaceIds { get; set; }
    public UId[][] PlaceObjects { get; set; }
    public SaveObjectState[] ObjectStates { get; set; }

    public SaveObject(int currentSceneIndex, int placesCount, int objectsCount)
    {
        CurrentSceneIndex = currentSceneIndex;
        PlaceIds = new UId[placesCount];
        PlaceObjects = new UId[placesCount][];
        ObjectStates = new SaveObjectState[objectsCount];
    }

    public void AddPlaceObjects(IPlace place)
    {
        PlaceIds[_lastPlaceId] = place.Id;
        PlaceObjects[_lastPlaceId] = place.GameObjects.Select(obj => obj.Id).ToArray();
        ++_lastPlaceId;
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
    public UId Id { get; set; }
    public UId ModelId { get; set; }
    public UId ResourceId { get; set; }
    public string BodyTypeName { get; set; }
    public byte[] Position { get; set; }
    public byte[] Scale { get; set; }
    public byte[] Rotation { get; set; }
    public byte[] Color { get; set; }
    public int VisibilityType { get; set; }
    public int Visual { get; set; }
    public ushort CollisionLayer { get; set; }
    public byte[] Material { get; set; }
    public byte[] Force { get; set; }
    public byte[] GravityForce { get; set; }
    public bool IsEnabled { get; set; }
    
    public SaveObjectState(IGameObject obj)
    {
        Id = obj.Id;
        ModelId = obj.Model.Id;
        ResourceId = obj.Transform.Texture.Id;
        BodyTypeName = obj.Body.GetType().FullName;
        Position = Vector.ToBytes(obj.Transform.Position);
        Scale = Vector.ToBytes(obj.Transform.Scale);
        Rotation = Vector.ToBytes(obj.Transform.Rotation);
        Color = Vector.ToBytes(obj.Transform.Color);
        VisibilityType = (int)obj.Transform.Visibility;
        Visual = (int)obj.Transform.Visual;
        CollisionLayer = obj.Body.CollisionLayer;
        Material = MaterialToBytes(obj.Body.Material);
        Force = Vector.ToBytes(obj.Body.Force);
        GravityForce = Vector.ToBytes(obj.Body.GravityForce);
        IsEnabled = obj.Body.IsEnabled;
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