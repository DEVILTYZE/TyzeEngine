using System.Collections.Generic;
using System.IO;
using System.Linq;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public class Resource : IResource
{
    public string Path { get; }
    public IReadOnlyList<Texture> Data { get; private set; }
    public bool LoadError { get; private set; }
    public bool Loaded { get; private set; }

    public Resource(string path) => Path = path;

    public void Load()
    {
        var isDirectory = File.GetAttributes(Path) == FileAttributes.Directory;

        if (isDirectory)
        {
            var filePaths = Directory.GetFiles(Path, "*", SearchOption.TopDirectoryOnly);
            Data = filePaths.Select(Load).ToList();
        }
        else if (File.Exists(Path))
            Data = new[] { Load(Path) };
    }

    private Texture Load(string filePath)
    {
        string path;
        bool isLoaded;
        
        if (!File.Exists(filePath))
        {
            path = ConstHelper.FileNotExists;
            isLoaded = false;
        }
        else
        {
            path = filePath;
            isLoaded = true;
        }

        var texture = new Texture(path);
        Loaded = true;
        
        if (!isLoaded)
            LoadError = true;

        return texture;
    }
}