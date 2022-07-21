using System;
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

public sealed class Model : IModel
{
    private readonly float[] _vectors;
    private Vector3 _position;
    private Vector3 _size;
    private Vector4 _color;
    private IVectorArray _texture;
    
    public string Name { get; }

    public Model(string name)
    {
        Name = name;
        LoadModelFromFile();
    }
    
    internal Model(SaveModelData saveData) : this(saveData.Name)
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
        
        for (var i = 0; i < _vectors.Length; i += 3)
        {
            _vectors[i] += x;
            _vectors[i + 1] += y;
            _vectors[i + 2] += z;
        }

        _position = tempVector;
    }

    public void ChangeSize(float x, float y, float z)
    {
        var tempVector = new Vector3(x, y, z);
        x = _size.X == 0 ? 0 : x / _size.X;
        y = _size.Y == 0 ? 0 : y / _size.Y;
        z = _size.Z == 0 ? 0 : z / _size.Z;
        
        for (var i = 0; i < _vectors.Length; i += 3)
        {
            _vectors[i] *= x;
            _vectors[i + 1] *= y;
            _vectors[i + 2] *= z;
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

    public (float[], float[]) GetVectorArray(bool withTexture = false)
        => withTexture
            ? (_vectors, _texture.GetArray())
            : (_vectors, new[] { _color.R, _color.G, _color.B, _color.A });

    public override string ToString()
        => $"model: {Name}\r\n" + string.Join(' ', _position) + "\r\n" + string.Join(' ', _size) + "\r\n" 
           + string.Join(' ', _color) + "\r\n" + string.Join(' ', _texture.GetArray()) + "\r\n" + (int)_texture.Type 
           + "\r\nend model;";

    private void ChangeColor(float r, float g, float b, float a)
    {
        _color.R = r;
        _color.G = g;
        _color.B = b;
        _color.A = a;
    }
    
    private void LoadModelFromFile()
    {
        throw new NotImplementedException();
    }
}