using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public class Model : IModel
{
    private bool _disposed;
    private uint[] _indices;
    private IVectorArray _texture = new VectorArray();
    private List<Vector3> _vertices = new();

    public UId Id { get; set; } = new();
    public IReadOnlyList<Vector3> Vertices => _vertices;
    public IEnumerable<Vector2> Vertices2D => Vertices.Select(vertex => vertex.Xy).ToArray();
    public string Directory { get; private set; }
    public string Name { get; private set; }
    public bool Loaded { get; private set; }

    protected Model((List<Vector3>, uint[], IVectorArray) coordinates)
    {
        _vertices = coordinates.Item1;
        _indices = coordinates.Item2;
        _texture = coordinates.Item3;
        Loaded = true;
    }
    
    public Model(string name, string directory = Constants.ModelsDirectory)
    {
        Directory = directory;
        Name = name;
    }
    
    ~Model() => Dispose(false);

    public void Load()
    {
        if (Loaded)
            return;

        string text;

        if (!File.Exists(Directory + Name))
            return;
        
        using (var sr = new StreamReader(Directory + Name))
        {
            text = sr.ReadToEnd();
        }

        const string pattern = "\"(-*\\d+\\.*\\d*\\s)+\\s*\"";
        var collection = Regex.Matches(text, pattern);
        var resultFloat = new[] { new List<float>(), new List<float>() };
        _indices = collection[0].Value.Split(new[] { ' ', '\"' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(uint.Parse).ToArray();
        
        for (var i = 0; i < resultFloat.Length; ++i)
            resultFloat[i].AddRange(collection[i + 2].Value.Split(new[] { ' ', '\"' }, 
                StringSplitOptions.RemoveEmptyEntries).Select(float.Parse));

        for (var i = 0; i < resultFloat[0].Count; i += 3)
            _vertices.Add(new Vector3(resultFloat[0][i], resultFloat[0][i + 1], resultFloat[0][i + 2]));

        for (var i = 0; i < resultFloat[1].Count; i += 2)
            _texture.Add(resultFloat[1][i], resultFloat[1][i + 1]);

        Loaded = true;
    }

    public (float[], uint[]) GetVectorArray(IGameObject obj)
    {
        float[] GetArrays(float[] texture, Vector4 color)
        {
            const int tStride = 2;
            var colorArray = new[] { color.X, color.Y, color.Z, color.W };
            var result = new List<float>(Vertices.Count * (texture.Length + Constants.Vector4Length));

            for (var i = 0; i < Vertices.Count; ++i)
            {
                result.AddRange(new[] { Vertices[i].X, Vertices[i].Y, Vertices[i].Z });
                result.AddRange(texture[(i * tStride)..((i + 1) * tStride)].Concat(colorArray));
            }
            
            return result.ToArray();
        }

        var texture = Enumerable.Repeat(new[] { -1f, -1 }, Vertices.Count).SelectMany(x => x).ToArray();

        switch (obj.Transform.Visual)
        {
            case BodyVisualType.Color:
                return (GetArrays(texture, obj.Transform.Color), _indices);
            case BodyVisualType.Texture:
                obj.Transform.Color = Constants.NullColor;
                break;
            case BodyVisualType.ColorAndTexture:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(obj.Transform.Visual), "Visual error.");
        }

        return (GetArrays(_texture.GetArray(), obj.Transform.Color), _indices);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public static IModel FindOrDefault(string name)
    {
        var isFound = Game.Models.TryGetValue(name, out var value);

        return isFound ? value : null;
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
    
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _vertices = null;
            _indices = null;
            _texture = null;
            Directory = null;
            Name = null;
        }

        _disposed = true;
    }
}