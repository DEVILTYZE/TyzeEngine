using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public class Model : IModel
{
    private bool _disposed;
    private IVectorArray _texture = new VectorArray();
    private List<Vector3> _vertices = new(), _normals = new();
    private List<uint> _indices = new();
    private float[] _array;

    public UId Id { get; set; } = new();
    public IReadOnlyList<float> Array => _array;
    public IReadOnlyList<Vector3> Vertices => _vertices;
    public IEnumerable<Vector2> Vertices2D => Vertices.Select(vertex => vertex.Xy).ToArray();
    public IReadOnlyList<uint> Indices => _indices;
    public IEnumerable<float> TextureCoordinates => _texture.GetArray();
    public IReadOnlyList<Vector3> Normals => _normals;
    public string Directory { get; private set; }
    public string Name { get; private set; }
    public bool Loaded { get; private set; }
    
    public Model(string name, string directory = Constants.ModelsDirectory)
    {
        Directory = directory;
        Name = name;
    }

    protected Model((List<Vector3>, List<Vector3>, IVectorArray, List<uint>) coordinates)
    {
        _vertices = coordinates.Item1;
        _normals = coordinates.Item2.Select(Vector3.NormalizeFast).ToList();
        _texture = coordinates.Item3;
        _indices = coordinates.Item4;
        MixArrays();
        Loaded = true;
    }
    
    ~Model() => Dispose(false);

    public void Load()
    {
        if (Loaded)
            return;

        string[] text;

        if (!File.Exists(Directory + Name))
            throw new FileNotFoundException("Model file not found.", Directory + Name);
        
        using (var sr = new StreamReader(Directory + Name))
        {
            text = sr.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        foreach (var str in text)
            ParseWavefrontString(str);

        Loaded = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public static IModel Find(string name)
    {
        var isFound = Game.Models.TryGetValue(name, out var value);

        if (isFound)
            return value;

        throw new Exception("Model not found.");
    }
    
    internal static IVectorArray GetDefaultTexture(IReadOnlyList<Vector3> vertices)
    {
        var result = new List<float>(vertices.Count * 2);

        for (var i = 0; i < vertices.Count; ++i)
        {
            result.Add((vertices[i].X + 1) / 2);
            result.Add((vertices[i].Y + 1) / 2);
        }

        return new VectorArray(result, ArrayType.TwoDimensions);
    }
    
    void IGameResource.Remove()
    {
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _vertices = null;
            _normals = null;
            _indices = null;
            _texture = null;
            Directory = null;
            Name = null;
        }

        _disposed = true;
    }

    private void MixArrays()
    {
        var result = new List<float>();

        for (var i = 0; i < _vertices.Count; ++i)
        {
            result.AddRange(new[] { _vertices[i].X, _vertices[i].Y, _vertices[i].Z });
            result.AddRange(new[] { _normals[i].X, _normals[i].Y, _normals[i].Z });
            result.AddRange(_texture[i]);
        }

        _array = result.ToArray();
    }

    private void ParseWavefrontString(string str)
    {
        var parts = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        const string vertex = "v";
        const string texture = "vt";
        const string normal = "vn";
        const string space = "vp";
        const string index = "f";

        switch (parts[0])
        {
            case vertex:
                var value = parts[1..].Select(float.Parse).ToArray();
                _vertices.Add(new Vector3(value[0], value[1], value[2]));
                break;
            case texture:
                value = parts[1..].Select(float.Parse).ToArray();
                _texture.Add(value[0], value[1]);
                break;
            case index:
                var indexValue = parts[1..]
                    .Select(localStr => localStr.Split('/', StringSplitOptions.RemoveEmptyEntries))
                    .SelectMany(localStr => localStr).Select(uint.Parse);
                _indices.AddRange(indexValue);
                break;
            case normal: // На будущее...
                break;
            case space:
                break;
        }
    }
}