using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;
using TyzeEngine.Resources;

namespace TyzeEngine.Objects;

public record struct SaveGameObjectData
{
    public Uid Id { get; }
    public Uid[] ResourceIds { get; }
    public Uid ModelId { get; }
    public Dictionary<int, bool> TriggerDictionary { get; }
    public Vector3 Position { get; }
    public Vector3 Size { get; }
    public Vector3 Rotation { get; }
    public Vector4 Color { get; }

    public SaveGameObjectData(Uid[] resourceIds, byte[] data)
    {
        const int count = 4;
        ResourceIds = resourceIds;
        Id = new Uid(BitConverter.ToUInt32(data));
        ModelId = new Uid(BitConverter.ToUInt32(data, sizeof(int)));
        var triggersCount = BitConverter.ToInt32(data, sizeof(int) * 2);
        TriggerDictionary = new Dictionary<int, bool>(triggersCount);
        
        for (var i = 0; i < triggersCount; ++i)
        {
            var intIndex = i * Constants.SizeOfTrigger + 3 * sizeof(int);
            var boolIndex = i * Constants.SizeOfTrigger + 4 * sizeof(int);
            TriggerDictionary.Add(BitConverter.ToInt32(data, intIndex), BitConverter.ToBoolean(data, boolIndex));
        }

        var str = Encoding.UTF8.GetString(data[(sizeof(int) * 3 + Constants.SizeOfTrigger * triggersCount)..]);
        var parts = str.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        var name = parts[0][8..];
        var floatArray = new List<float[]>(count);
        
        for (var i = 0; i < count; ++i)
            floatArray.Add(parts[i + 1].Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(float.Parse).ToArray());
        
        Position = new Vector3(floatArray[0][0], floatArray[0][1], floatArray[0][2]);
        Size = new Vector3(floatArray[1][0], floatArray[1][1], floatArray[1][2]);
        Rotation = new Vector3(floatArray[2][0], floatArray[2][1], floatArray[2][2]);
        Color = new Vector4(floatArray[3][0], floatArray[3][1], floatArray[3][2], floatArray[3][3]);
    }
}

public abstract class GameObject : IGameObject, ISaveable, ICloneable, IDisposable
{
    private bool _disposed;
    private IObjectPhysics _physics;

    ArrayObject IGameObject.ArrayObject { get; set; }

    BufferUsageHint IGameObject.DrawType
    {
        get
        {
            if (!SaveStatus && (!Physics.IsEnabled || Physics.GravityForce.Length <= 0))
                return BufferUsageHint.StaticDraw;
            
            if (Visibility is VisibilityType.Collapsed or VisibilityType.Hidden)
                return BufferUsageHint.StaticDraw;

            if (Physics.IsEnabled && Physics.GravityForce.Length > 0)
                return BufferUsageHint.DynamicDraw;

            if (Scripts.Count > 0 && Triggers.Count > 0)
                return BufferUsageHint.StreamDraw;

            return BufferUsageHint.DynamicDraw;
        }
    }

    public Uid Id { get; protected init; }
    public Uid ModelId { get; private set; }
    public IReadOnlyList<Uid> ResourceIds { get; private set; }

    public IObjectPhysics Physics
    {
        get => _physics;
        set
        {
            value.Object = this;
            _physics = value.Clone();
        }
    }
    
    public List<ITrigger> Triggers { get; }
    public List<IScript> Scripts { get; }
    public Vector3 Position { get; private set; }
    public Vector3 Size { get; private set; }
    public Vector3 Rotation { get; private set; }
    public Vector4 Color { get; private set; }
    public bool HasTexture { get; private set; }
    public VisibilityType Visibility { get; private set; }
    public bool SaveStatus { get; }

    protected GameObject(Uid modelId, IReadOnlyList<Uid> resourceIds = null, IObjectPhysics physics = null, bool notSave = false)
    {
        Id = new Uid();
        ModelId = modelId;
        Physics = physics is null ? new RectanglePhysics() : physics.Clone();
        Physics.Object = this;
        SaveStatus = !notSave;
        ResourceIds = resourceIds is null ? new List<Uid>() : resourceIds.ToList();
        Triggers = new List<ITrigger>();
        Scripts = new List<IScript>();
        Position = Constants.DefaultPosition;
        Size = Constants.DefaultSize2D;
        Rotation = Constants.DefaultRotation;
        Color = Constants.DefaultColor;
        
        if (resourceIds is not null) 
            ResourceIds = resourceIds.ToList();
    }

    protected GameObject(SaveGameObjectData saveData, IObjectPhysics physics = null, bool notSave = false) 
        : this(saveData.ModelId, saveData.ResourceIds, physics, notSave)
    {
        Id = saveData.Id;
        Position = saveData.Position;
        Size = saveData.Size;
        Rotation = saveData.Rotation;
        Color = saveData.Color;
        
        foreach (var trigger in Triggers.Where(trigger => saveData.TriggerDictionary.ContainsKey(trigger.Id)))
            trigger.IsTriggered = saveData.TriggerDictionary[trigger.Id];
    }
    
    ~GameObject() => Dispose(false);
    
    public void TranslateTo(float x, float y, float z) => Position = new Vector3(x, y, z);

    public void ScaleTo(float x, float y, float z)
    {
        if (x < 0 || y < 0 || z < 0)
            return;

        Size = new Vector3(x, y, z);
    }

    public void RotateTo(float x, float y, float z) => Rotation = new Vector3(x, y, z);

    public void Translate(float x, float y, float z) => TranslateTo(Position.X + x, Position.Y + y, Position.Z + z);

    public void Scale(float x, float y, float z) => ScaleTo(Size.X * x, Size.Y * y, Size.Z * z);

    public void Rotate(float x, float y, float z) => RotateTo(Rotation.X + x, Rotation.Y + y, Rotation.Z + z);

    public void ChangeColor(byte r, byte g, byte b, byte a) => Color = new Vector4(
        (float)r / byte.MaxValue, 
        (float)g / byte.MaxValue, 
        (float)b / byte.MaxValue,
        (float)a / byte.MaxValue
    );

    public void RemoveColor() => Color = Constants.NullColor;
    public void ChangeTextureStatus(bool isEnabled, bool withColor = false)
    {
        HasTexture = isEnabled;
        
        if (!withColor)
            RemoveColor();
    }

    public void ChangeVisibility(VisibilityType newType) => Visibility = newType;

    public void EnableResources(Dictionary<Uid, IResource> resources)
    {
        foreach (var id in ResourceIds)
            resources[id].Enable();
    }

    public void DisableResources(Dictionary<Uid, IResource> resources)
    {
        foreach (var id in ResourceIds)
            resources[id].Disable();
    }

    public byte[] GetSaveData()
    {
        var id = BitConverter.GetBytes(Id.Value);
        var modelId = Encoding.UTF8.GetBytes(ModelId.ToString());
        var triggerList = Triggers.Where(trigger => ((ISaveable)trigger).SaveStatus).ToList();
        var triggerCount = BitConverter.GetBytes(triggerList.Count);
        var triggers = triggerList.SelectMany(trigger => ((ISaveable)trigger).GetSaveData()).ToArray();
        var objectInfo = Encoding.UTF8.GetBytes(ToString());

        return id.Concat(modelId).Concat(triggerCount).Concat(triggers).Concat(objectInfo).ToArray();
    }
    
    public override string ToString()
        => $"object: {Id.Value}\r\n" + 
           string.Join(' ', Position) + "\r\n" + 
           string.Join(' ', Size) + "\r\n" + 
           string.Join(' ', Rotation) + "\r\n" +
           string.Join(' ', Color) + 
           "\r\nend object;";

    object ICloneable.Clone() => Clone();

    protected abstract IGameObject Clone();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // something...
        }

        ResourceIds = null;
        _disposed = true;
    }
}
