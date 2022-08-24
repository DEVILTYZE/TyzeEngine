using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace TyzeEngine;

public class Saver
{
    private string _saveFileName;

    public SaveStatus LastSaveStatus { get; private set; } = SaveStatus.Unknown;

    public void Save(byte[] data, string filePath = null)
    {
        LastSaveStatus = SaveStatus.Unknown;
        _saveFileName = string.IsNullOrEmpty(filePath) 
            ? Path.Combine(Constants.SavesDirectory, Constants.DefaultSaveName + Constants.SaveExtension)
            : filePath;

        ThreadPool.QueueUserWorkItem(_ => Save((object)data));
    }

    public static byte[] Load(string filePath = null)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath), "File path is null.");

        if (!File.Exists(filePath))
            throw new ArgumentException($"Save file {Path.GetFileName(filePath)} doesn't exists.", nameof(filePath));

        const int defaultLength = 256;
        var byteList = new List<byte>();
        using var fs = File.Open(filePath, FileMode.Open);
        
        while (fs.CanRead)
        {
            var data = new byte[defaultLength];
            var length = fs.Read(data, 0, data.Length);

            if (length < defaultLength)
            {
                data = data[..length];
                byteList.AddRange(data);
                break;
            }
                
            byteList.AddRange(data);
        }

        return byteList.ToArray();
    }

    private void Save(object obj)
    {
        try
        {
            var data = (byte[])obj;
            var stream = File.OpenWrite(_saveFileName);
            SaveInFile(data, stream);
            stream.Close();
            LastSaveStatus = SaveStatus.Succeed;
        }
        catch (Exception)
        {
            LastSaveStatus = SaveStatus.Error;
        }
    }

    protected virtual void SaveInFile(object data, FileStream stream)
    {
        var dataArray = (byte[])data;
        stream.Write(dataArray, 0, dataArray.Length);
    }
}