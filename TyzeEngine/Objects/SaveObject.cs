using System;
using System.Linq;
using System.Text.Json;
using OpenTK.Mathematics;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;
using TyzeEngine.Materials;

namespace TyzeEngine.Objects;

[Serializable]
public struct SaveObject : ISaveable
{
    private int _lastSpaceId = 0;
    private int _lastObjectId = 0;
    
    public int CurrentSceneIndex { get; set; }
    public UId[] SpaceIds { get; set; }
    public UId[][] SpaceObjects { get; set; }
    public SaveObjectState[] ObjectStates { get; set; }

    public SaveObject(int currentSceneIndex, int spacesCount, int objectsCount)
    {
        CurrentSceneIndex = currentSceneIndex;
        SpaceIds = new UId[spacesCount];
        SpaceObjects = new UId[spacesCount][];
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
        ResourceId = obj.Visual.Texture.Id;
        BodyTypeName = obj.Body.GetType().FullName;
        Position = Vector.ToBytes(obj.Transform.Position);
        Scale = Vector.ToBytes(obj.Transform.Scale);
        Rotation = Vector.ToBytes(obj.Transform.Rotation);
        Color = Vector.ToBytes(Color4.ToXyz(obj.Visual.Color));
        VisibilityType = (int)obj.Visual.Visibility;
        Visual = (int)obj.Visual.Type;
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