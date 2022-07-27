using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

internal record struct SaveModelData
{
    internal string Name { get; }
    internal Vector3 Position { get; }
    internal Vector3 Size { get; }
    internal Vector3 Rotation { get; }
    internal Vector4 Color { get; }
    internal IVectorArray Texture { get; }

    internal SaveModelData(string name, Vector3 position, Vector3 size, Vector3 rotation, Vector4 color, IVectorArray texture)
    {
        Name = name;
        Position = position;
        Size = size;
        Rotation = rotation;
        Color = color;
        Texture = texture;
    }
}

public class Model : IModel
{
    private float[] _vertices;
    private uint[] _indices;
    private Vector4 _color;
    private IVectorArray _texture;

    public string Directory { get; }
    public string Name { get; }
    public bool LoadError { get; private set; }
    public Vector3 Position { get; private set; }
    public Vector3 Size { get; private set; }
    public Vector3 Rotation { get; private set; }
    public VisibilityType Visibility { get; private set; }

    public static IModel Point => new Model(DefaultModels.GetPoint(), Constants.DefaultSize2D);
    public static IModel Triangle => new Model(DefaultModels.GetTriangle(), Constants.DefaultSize2D);
    public static IModel Rectangle => new Model(DefaultModels.GetRectangle(), Constants.DefaultSize2D);
    public static IModel Circle => new Model(DefaultModels.GetCircle(), Constants.DefaultSize2D);
    public static IModel Cube => new Model(Constants.TriangleModelName, Constants.DefaultModelsDirectory);

    private Model((float[], uint[]) coordinates, Vector3 scale)
    {
        _vertices = coordinates.Item1;
        _texture = null;
        _indices = coordinates.Item2;
        Position = Constants.DefaultPosition;
        Size = scale;
        _color = Constants.DefaultColor;
    }
    
    public Model(string name, string directory = Constants.ModelsDirectory)
    {
        Directory = directory;
        Name = name;
        LoadError = false;
        LoadModelFromFile();
    }

    internal Model(in SaveModelData saveData) : this(saveData.Name)
    {
        Scale(saveData.Size.X, saveData.Size.Y, saveData.Size.Z);
        Translate(saveData.Position.X, saveData.Position.Y, saveData.Position.Z);
        Rotate(saveData.Rotation.X, saveData.Rotation.Y, saveData.Rotation.Z);
        ChangeTexture(saveData.Texture);
        _color.X = saveData.Color.X;
        _color.Y = saveData.Color.Y;
        _color.Z = saveData.Color.Z;
        _color.W = saveData.Color.W;
    }
    
    public void TranslateTo(float x, float y, float z) => Position = new Vector3(x, y, z);

    public void ScaleTo(float x, float y, float z)
    {
        if (x < 0 || y < 0 || z < 0)
            return;

        Size = new Vector3(x, y, z);
    }

    public void RotateTo(float x, float y, float z) => Rotation = new Vector3(x, y, z);

    public void Translate(float x, float y, float z) => TranslateTo(Position.X + x, Position.Y + y, Position.Z + z);

    public void Scale(float x, float y, float z) => ScaleTo(Size.X * x, Size.Y * y, Size.Z * z);

    public void Rotate(float x, float y, float z) => RotateTo(Rotation.X + x, Rotation.Y + y, Rotation.Z + z); 

    public void ChangeColor(byte r, byte g, byte b, byte a)
    {
        _color.X = (float)r / byte.MaxValue;
        _color.Y = (float)g / byte.MaxValue;
        _color.Z = (float)b / byte.MaxValue;
        _color.W = (float)a / byte.MaxValue;
    }

    public void RemoveColor() => _color.W = 0;

    public void ChangeTexture(IVectorArray array) => _texture = array;

    public void AddDefaultTexture(bool withColor = false)
    {
        _texture = new VectorArray(DefaultModels.GetDefaultTexture(_vertices), ArrayType.TwoDimensions);

        if (!withColor)
            RemoveColor();
    }

    public void RemoveTexture() => _texture = null;

    public void ChangeVisibility(VisibilityType newType) => Visibility = newType;

    public (float[], uint[]) GetVectorArray()
    {
        float[] GetArrays(float[] texture, float[] colorArray, bool withTexture)
        {
            const int vStride = 3;
            const int tStride = 2;
            var count = _vertices.Length / Constants.VertexLength;
            var result = new List<float>(count * (texture.Length + Constants.ColorLength));
            
            if (withTexture)
                for (var i = 0; i < count; ++i)
                {
                    result.AddRange(_vertices[(i * vStride)..((i + 1) * vStride)]);
                    result.AddRange(texture[(i * tStride)..((i + 1) * tStride)].Concat(colorArray));
                }
            else
                for (var i = 0; i < count; ++i)
                {
                    result.AddRange(_vertices[(i * vStride)..((i + 1) * vStride)]);
                    result.AddRange(texture.Concat(colorArray));
                }

            return result.ToArray();
        }
        
        var texture = new[] { -1f, -1f };
        var colorArray = new[] { _color.X, _color.Y, _color.Z, _color.W };

        return Visibility switch
        {
            VisibilityType.Hidden => (GetArrays(texture, new[] { 0f, 0, 0, 0 }, false), _indices),
            VisibilityType.Collapsed => (new[] { 0f, 0, 0, -1, -1, 0, 0, 0, 0 }, new uint[] { 1 }),
            VisibilityType.Visible or _ => _texture is null
                ? (GetArrays(texture, colorArray, false), _indices)
                : (GetArrays(_texture.GetArray(), colorArray, true), _indices)
        };
    }

    public override string ToString()
        => $"model: {Name}\r\n" + 
           string.Join(' ', Position) + "\r\n" + 
           string.Join(' ', Size) + "\r\n" + 
           string.Join(' ', Rotation) + "\r\n" +
           string.Join(' ', _color) + "\r\n" + 
           string.Join(' ', _texture.GetArray()) + "\r\n" + 
           (int)_texture.Type + 
           "\r\nend model;";

    private void LoadModelFromFile()
    {
        void SetField(int index, MatchCollection matches)
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
                    Size = new Vector3(doubleValue[0], doubleValue[1], doubleValue[2]);
                    break;
                case 3:
                    Position = new Vector3(doubleValue[0], doubleValue[1], doubleValue[2]);
                    break;
                default:
                    _color = new Vector4(doubleValue[0], doubleValue[1], doubleValue[2], doubleValue[3]);
                    break;
            }
        }
        
        var path = Path.Combine(Directory, Name + Constants.ModelExtension);

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
}