using System;

namespace TyzeEngine.Resources;

public abstract class Resource : IResource, IDisposable
{
    private bool _disposed;
    
    public string Path { get; private set; }
    public bool LoadError { get; protected set; }

    protected Resource(string path)
    {
        Path = path;
    }

    ~Resource() => Dispose(false);

    public abstract void Load();
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        Path = null;
    }
}