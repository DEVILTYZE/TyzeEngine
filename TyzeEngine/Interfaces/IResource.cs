using System.Collections.Generic;

namespace TyzeEngine.Interfaces;

public interface IResource
{
    string Path { get; }
    IReadOnlyList<Texture> Data { get; }
    bool LoadError { get; }
    bool Loaded { get; }

    void Load();
}