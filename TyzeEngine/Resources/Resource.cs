using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Resources;

public abstract class Resource : IResource
{
    protected bool Disposed;

    public UId Id { get; set; } = new();

    public int Handle { get; protected set; } = -1;
    public string Path { get; private set; }
    public bool LoadError { get; protected set; }

    protected Resource(string path) => Path = path;

    ~Resource() => Dispose(false);

    public abstract void Load();
    
    public abstract void Enable();
    
    public abstract void Disable();

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    void IGameResource.Remove()
    {
    }

    protected virtual void Dispose(bool disposing)
    {
        if (Disposed)
            return;

        if (disposing)
        {
            Handle = 0;
            Path = null;
        }

        Disposed = true;
    }
    
    /// <summary>
    /// Ищет ресурс по имени среди всех добавленных в игру ресурсов.
    /// </summary>
    /// <param name="name">Имя ресурса.</param>
    /// <returns>Объект ресурса, приведённый к типу IResource.</returns>
    /// <exception cref="ArgumentNullException">Имя равно null.</exception>
    /// <exception cref="Exception">Ресурс не найден.</exception>
    public static IResource Find([NotNull] string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name), "Name was null.");
        
        var isFound = Game.Resources.TryGetValue(name, out var value);

        return isFound ? value : throw new Exception("Resource not found.");
    }
    
    public static IResource CreateObject(string path)
    {
        var extension = System.IO.Path.GetExtension(path);

        if (string.IsNullOrEmpty(extension))
            throw new Exception("Can't determine the format.");

        bool IsEquals(string str1, string str2) => string.CompareOrdinal(str1, str2) == 0;

        if (Constants.ImageExtensions.Any(localExtension => IsEquals(localExtension, extension)))
            return new Texture(path);

        if (Constants.AudioExtensions.Any(localExtension => IsEquals(localExtension, extension)))
            return new Audio(path);

        if (Constants.VideoExtensions.Any(localExtension => IsEquals(localExtension, extension)))
            return new Video(path);
        
        throw new Exception("Can't determine the format.");
    }
}