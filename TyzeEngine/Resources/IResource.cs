﻿using System;
using System.Linq;

namespace TyzeEngine.Resources;

public interface IResource : IDisposable
{
    Uid Id { get; }
    int Handle { get; }
    string Path { get; }
    bool LoadError { get; }

    void Load();
    void Enable();
    void Disable();

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