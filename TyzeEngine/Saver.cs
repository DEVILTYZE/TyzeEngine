using System.Collections.Generic;
using System.IO;
using System.Threading;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;

namespace TyzeEngine;

public class Saver
{
    private string _saveFileName;

    public void SaveObjects(IReadOnlyList<IGameObject> objects)
    {
        _saveFileName = Path.Combine(ConstHelper.SavesDirectory, ConstHelper.DefaultSaveName + ConstHelper.SaveExtension);

        ThreadPool.QueueUserWorkItem(_ => SaveObjects((object)objects));
    }

    private void SaveObjects(object obj)
    {
        var objects = (IReadOnlyList<IGameObject>)obj;
        var stream = File.OpenWrite(_saveFileName);
        
        foreach (var gameObject in objects)
            Save(gameObject, stream);
        
        stream.Close();
    }

    protected virtual void Save(IGameObject obj, FileStream stream)
    {
        var data = ((ISaveable)obj).GetSaveData();
        stream.Write(data, 0, data.Length);
    }
}