using System;
using System.Linq;

namespace TyzeEngine.Resources;

public interface IResource
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

        if (Constants.ImageExtensions.Any(thisExtension => IsEquals(thisExtension, extension)))
            return new Texture(path);

        if (Constants.AudioExtensions.Any(thisExtension => IsEquals(thisExtension, extension)))
            return new Audio(path);

        if (Constants.VideoExtensions.Any(thisExtension => IsEquals(thisExtension, extension)))
            return new Video(path);
        
        throw new Exception("Can't determine the format.");
    }
}