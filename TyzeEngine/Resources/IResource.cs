using System;
using System.Linq;

namespace TyzeEngine.Resources;

public interface IResource
{
    string Path { get; }
    bool LoadError { get; }

    void Load();
    void Enable();
    void Disable();

    internal static IResource GetByPath(string path)
    {
        var extension = System.IO.Path.GetExtension(path);

        if (string.IsNullOrEmpty(extension))
            throw new Exception("Can't determine the format.");

        bool IsEquals(string str1, string str2) => string.CompareOrdinal(str1, str2) == 0;

        if (ConstHelper.ImageExtensions.Any(thisExtension => IsEquals(thisExtension, extension)))
            return new Texture(path);

        if (ConstHelper.AudioExtensions.Any(thisExtension => IsEquals(thisExtension, extension)))
            return new Audio(path);

        if (ConstHelper.VideoExtensions.Any(thisExtension => IsEquals(thisExtension, extension)))
            return new Video(path);
        
        throw new Exception("Can't determine the format.");
    }
}