using System;
using System.IO;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Loggers;

public class FileLogger<T> : ILogger<T>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly object Locker = new();
    private const string Folder = "logs";
    private const string Extension = ".log";
    
    public void Log(T data)
    {
        var date = DateTime.Now;
        var dateString = date.ToString("yyyy_MM_dd_hh_mm_ss");
        var count = 0;

        while (File.Exists(Path.Combine(Folder, $"{dateString}({count}){Extension}")))
            ++count;

        using var sw = new StreamWriter(Path.Combine(Folder, $"{dateString}({count}){Extension}"));
        sw.WriteLine(date.ToString("G"));
        sw.WriteLine(data.ToString());
    }

    public void LogSync(T data)
    {
        lock(Locker)
        {
            Log(data);
        }
    }
    
    public override string ToString() => typeof(T).Name + " file logger";
}