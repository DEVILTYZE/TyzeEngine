using System;
using System.Linq;

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
    
    public static IResource Find(string name)
    {
        var isFound = Game.Resources.TryGetValue(name, out var value);

        return isFound ? value : null;
    }
    
    public static IResource GetByPath(string path)
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