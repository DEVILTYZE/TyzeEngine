using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine;

public readonly record struct SaveGameObjectData // TODO: SAVE BODY TYPE
{
    private readonly string _objectInfo;
    
    public Uid Id { get; }
    public Uid[] ResourceIds { get; }
    public IModel Model { get; }
    public Dictionary<Uid, bool> TriggerDictionary { get; }
    public Vector3 Position { get; }
    public Vector3 Size { get; }
    public Vector3 Rotation { get; }
    public Vector4 Color { get; }
    public string BodyName { get; }
    
    public SaveGameObjectData(IGameObject obj)
    {
        Id = obj.Id;
        ResourceIds = obj.ResourceIds.ToArray();
        Model = obj.Model;
        var triggerList = obj.Triggers.Where(trigger => trigger.SaveStatus).ToList();
        TriggerDictionary = new Dictionary<Uid, bool>(triggerList.Select(trigger 
            => new KeyValuePair<Uid, bool>(trigger.Id, trigger.IsTriggered)));
        Position = obj.Body.Position;
        Size = obj.Body.Scale;
        Rotation = obj.Body.Rotation;
        Color = obj.Body.Color;
        BodyName = obj.Body.GetType().ToString();
        _objectInfo = obj.ToString();
    }
    
    public SaveGameObjectData(Uid[] resourceIds, byte[] data)
    {
        const int count = 4;
        ResourceIds = resourceIds;
        Id = new Uid(BitConverter.ToUInt32(data));
        //Model = new Uid(BitConverter.ToUInt32(data, sizeof(int)));
        Model = new Model(string.Empty, string.Empty);
        var triggersCount = BitConverter.ToInt32(data, sizeof(int) * 2);
        TriggerDictionary = new Dictionary<Uid, bool>(triggersCount);
        
        for (var i = 0; i < triggersCount; ++i)
        {
            var intIndex = i * Constants.SizeOfTrigger + 3 * sizeof(int);
            var boolIndex = i * Constants.SizeOfTrigger + 4 * sizeof(int);
            TriggerDictionary.Add(new Uid(BitConverter.ToInt32(data, intIndex)), BitConverter.ToBoolean(data, boolIndex));
        }

        var str = Encoding.UTF8.GetString(data[(sizeof(int) * 3 + Constants.SizeOfTrigger * triggersCount)..]);
        var parts = str.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        var floatArray = new List<float[]>(count);
        
        for (var i = 0; i < count; ++i)
            floatArray.Add(parts[i + 1].Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(float.Parse).ToArray());
        
        Position = new Vector3(floatArray[0][0], floatArray[0][1], floatArray[0][2]);
        Size = new Vector3(floatArray[1][0], floatArray[1][1], floatArray[1][2]);
        Rotation = new Vector3(floatArray[2][0], floatArray[2][1], floatArray[2][2]);
        Color = new Vector4(floatArray[3][0], floatArray[3][1], floatArray[3][2], floatArray[3][3]);
        BodyName = parts[count + 1];
        _objectInfo = string.Empty;
    }

    public byte[] GetData()
    {
        var id = BitConverter.GetBytes(Id.Value);
        var modelId = Encoding.UTF8.GetBytes(Model.ToString());
        var triggerCount = BitConverter.GetBytes(TriggerDictionary.Count);
        var triggers = TriggerDictionary.SelectMany(pair => BitConverter.GetBytes(pair.Key.Value)
            .Concat(BitConverter.GetBytes(pair.Value))).ToArray();
        var objectInfo = Encoding.UTF8.GetBytes(_objectInfo);

        return id.Concat(modelId).Concat(triggerCount).Concat(triggers).Concat(objectInfo).ToArray();
    }
}

public class Saver
{
    private string _saveFileName;

    public void SaveObjects(IReadOnlyList<IGameObject> objects)
    {
        _saveFileName = Path.Combine(Constants.SavesDirectory, Constants.DefaultSaveName + Constants.SaveExtension);

        ThreadPool.QueueUserWorkItem(_ => SaveObjects((object)objects));
    }

    private void SaveObjects(object obj)
    {
        var objects = (IReadOnlyList<IGameObject>)obj;
        var stream = File.OpenWrite(_saveFileName);
        
        foreach (var gameObject in objects)
            Save(new SaveGameObjectData(gameObject), stream);
        
        stream.Close();
    }

    protected virtual void Save(object saveData, FileStream stream)
    {
        var data = ((SaveGameObjectData)saveData).GetData();
        stream.Write(data, 0, data.Length);
    }
}