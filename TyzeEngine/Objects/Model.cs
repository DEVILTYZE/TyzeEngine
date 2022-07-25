using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

internal record struct Vector3(float X, float Y, float Z);
internal record struct Vector4(float R, float G, float B, float A);

internal record struct SaveModelData
{
    internal string Name { get; }
    internal Vector3 Position { get; }
    internal Vector3 Size { get; }
    internal Vector4 Color { get; }
    internal IVectorArray Texture { get; }

    internal SaveModelData(string name, Vector3 position, Vector3 size, Vector4 color, IVectorArray texture)
    {
        Name = name;
        Position = position;
        Size = size;
        Color = color;
        Texture = texture;
    }
}

public record struct VectorObject
{
    public float[] Vertices { get; }
    public float[] VisualArray { get; }
    public uint[] Indices { get; }

    public VectorObject(float[] vertices, float[] visualArray, uint[] indices)
    {
        Vertices = vertices;
        VisualArray = visualArray;
        Indices = indices;
    }
}

public sealed class Model : IModel
{
    private float[] _vertices;
    private uint[] _indices;
    private Vector3 _position;
    private Vector3 _size;
    private Vector4 _color;
    private IVectorArray _texture;
    private bool _isDefaultModel;
    
    public string Name { get; }
    public bool LoadError { get; private set; }

    public static IModel Point => new Model(ConstHelper.PointModelName) { _isDefaultModel = true };
    public static IModel Triangle => new Model(ConstHelper.TriangleModelName) { _isDefaultModel = true };
    public static IModel Rectangle => new Model(ConstHelper.RectangleModelName) { _isDefaultModel = true };
    public static IModel Circle => new Model(ConstHelper.CircleModelName) { _isDefaultModel = true };
    public static IModel Cube => new Model(ConstHelper.TriangleModelName) { _isDefaultModel = true };

    public Model(string name)
    {
        Name = name;
        LoadError = false;
        LoadModelFromFile();
    }
    
    internal Model(in SaveModelData saveData) : this(saveData.Name)
    {
        ChangeSize(saveData.Size.X, saveData.Size.Y, saveData.Size.Z);
        ChangePosition(saveData.Position.X, saveData.Position.Y, saveData.Position.Z);
        ChangeColor(saveData.Color.R, saveData.Color.G, saveData.Color.B, saveData.Color.A);
        ChangeTexture(saveData.Texture);
    }

    public void ChangePosition(float x, float y, float z)
    {
        var tempVector = new Vector3(x, y, z);
        x -= _position.X;
        y -= _position.Y;
        z -= _position.Z;
        
        for (var i = 0; i < _vertices.Length; i += 3)
        {
            _vertices[i] += x;
            _vertices[i + 1] += y;
            _vertices[i + 2] += z;
        }

        _position = tempVector;
    }

    public void ChangeSize(float x, float y, float z)
    {
        var tempVector = new Vector3(x, y, z);
        x = _size.X == 0 ? 0 : x / _size.X;
        y = _size.Y == 0 ? 0 : y / _size.Y;
        z = _size.Z == 0 ? 0 : z / _size.Z;
        
        for (var i = 0; i < _vertices.Length; i += 3)
        {
            _vertices[i] *= x;
            _vertices[i + 1] *= y;
            _vertices[i + 2] *= z;
        }

        _size = tempVector;
    }

    public void ChangeColor(byte r, byte g, byte b, byte a)
    {
        _color.R = (float)r / byte.MaxValue;
        _color.G = (float)g / byte.MaxValue;
        _color.B = (float)b / byte.MaxValue;
        _color.A = (float)a / byte.MaxValue;
    }

    public void ChangeTexture(IVectorArray array) => _texture = array;

    public VectorObject GetVectorArray() => new(_vertices, GetVisualArray(), _indices);

    public override string ToString()
        => $"model: {Name}\r\n" + 
           string.Join(' ', _position) + "\r\n" + 
           string.Join(' ', _size) + "\r\n" + 
           string.Join(' ', _color) + "\r\n" + 
           string.Join(' ', _texture.GetArray()) + "\r\n" + 
           (int)_texture.Type + 
           "\r\nend model;";

    private void ChangeColor(float r, float g, float b, float a)
    {
        _color.R = r;
        _color.G = g;
        _color.B = b;
        _color.A = a;
    }

    private float[] GetVisualArray()
    {
        var texture = _texture is null ? new[] { -1.0f, -1, -1, -1, -1, -1, -1, -1 } : _texture.GetArray();
        const int stride = 2;
        var result = new List<float>(texture.Length + ConstHelper.ColorLength);
        var colorArray = new[] { _color.R, _color.G, _color.B, _color.A };
        
        for (var i = 0; i < ConstHelper.ColorLength; ++i)
            result.AddRange(texture[(i * stride)..((i + 1) * stride)].Concat(colorArray));

        return result.ToArray();
    }

    private void LoadModelFromFile()
    {
        var directory = _isDefaultModel ? ConstHelper.DefaultModelsDirectory : ConstHelper.ModelsDirectory;
        var path = Path.Combine(directory, Name + ConstHelper.ModelExtension);

        if (!File.Exists(path))
        {
            LoadError = true;
            return;
        }

        var text = File.ReadAllText(path);
        const string pattern = @"(((-?\d.\d(\s|))|\d(\s|))+\r\n)+";
        var matches = Regex.Matches(text, pattern);
        
        for (var i = 0; i < matches.Count; ++i)
            SetField(i, matches);
    }

    private void SetField(int index, MatchCollection matches)
    {
        var value = matches[index].Value.Split(new[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        var doubleValue = value.Select(float.Parse).ToArray();
        
        switch (index)
        {
            case 0:
                _vertices = doubleValue;
                break;
            case 1:
                _indices = value.Select(uint.Parse).ToArray();
                break;
            case 2:
                _size = new Vector3(doubleValue[0], doubleValue[1], doubleValue[2]);
                break;
            case 3:
                _position = new Vector3(doubleValue[0], doubleValue[1], doubleValue[2]);
                break;
            default:
                _color = new Vector4(doubleValue[0], doubleValue[1], doubleValue[2], doubleValue[3]);
                break;
        }
    }
}