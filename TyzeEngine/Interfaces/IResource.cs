using System;

namespace TyzeEngine.Interfaces;

public interface IResource : IDisposable, IGameResource
{
    int Handle { get; }
    string Path { get; }
    bool LoadError { get; }

    void Load();
    void Enable();
    void Disable();
}