using System;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Loggers;

public class ConsoleLogger<T> : ILogger<T>
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly object Locker = new();
    
    public void Log(T data)
    {
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write("LOG:");
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(" " + data);
    }

    public void LogSync(T data)
    {
        lock (Locker)
        {
            Log(data);
        }
    }

    public override string ToString() => typeof(T).Name + " console logger";
}