using System;

namespace TyzeEngine.Resources;

public abstract class Resource : IResource, IDisposable
{
    protected bool Disposed;

    public Uid Id { get; }
    public int Handle { get; protected set; }
    public string Path { get; private set; }
    public bool LoadError { get; protected set; }

    protected Resource(string path)
    {
        Handle = -1;
        Path = path;
        Id = new Uid();
    }

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

        Path = null;
        Disposed = true;
    }
}