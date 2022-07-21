using System;

namespace TyzeEngine.Resources;

public class Audio : Resource, IDisposable
{
    public Audio(string path) : base(path)
    {
    }

    public override void Load()
    {
        throw new NotImplementedException();
    }

    protected override void Dispose(bool disposing)
    {
        throw new NotImplementedException();
    }
}