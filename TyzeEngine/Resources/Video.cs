using System;

namespace TyzeEngine.Resources;

public class Video : Resource
{
    public Video(string path) : base(path)
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