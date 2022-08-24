using System;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Resources;

public interface IResource : IDisposable, IIdObject
{
    int Handle { get; }
    string Path { get; }
    bool LoadError { get; }

    void Load();
    void Enable();
    void Disable();
}