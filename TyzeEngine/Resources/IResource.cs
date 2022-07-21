using System;
using System.Linq;

namespace TyzeEngine.Resources;

public interface IResource
{
    string Path { get; }
    bool LoadError { get; }

    void Load();

    internal static IResource GetByPath(string path)
    {
        var extension = System.IO.Path.GetExtension(path);

        if (string.IsNullOrEmpty(extension))
            throw new Exception("Can't determine the format.");

        if (ConstHelper.ImageExtensions.Any(thisExtension => string.CompareOrdinal(thisExtension, extension) == 0))
            return new Texture(path);

        if (ConstHelper.AudioExtensions.Any(thisExtension => string.CompareOrdinal(thisExtension, extension) == 0))
            return new Audio(path);

        if (ConstHelper.VideoExtensions.Any(thisExtension => string.CompareOrdinal(thisExtension, extension) == 0))
            return new Video(path);
        
        throw new Exception("Can't determine the format.");
    }
}