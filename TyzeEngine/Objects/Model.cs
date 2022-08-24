﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public class Model : IModel
{
    private bool _disposed;
    private uint[] _indices;
    private IVectorArray _texture;

    public UId Id { get; set; } = new();
    public IReadOnlyList<Vector3> Vertices { get; private set; }
    public IEnumerable<Vector2> Vertices2D => Vertices.Select(vertex => vertex.Xy).ToArray();
    public string Directory { get; private set; }
    public string Name { get; private set; }
    public bool Loaded { get; private set; }

    public static IModel Point => new Model(DefaultModels.GetPoint()) { Loaded = true };
    public static IModel Triangle => new Model(DefaultModels.GetTriangle()) { Loaded = true };
    public static IModel Rectangle => new Model(DefaultModels.GetRectangle()) { Loaded = true };
    public static IModel Circle => new Model(DefaultModels.GetCircle()) { Loaded = true };
    // public static IModel Cube => new Model(Constants.TriangleModelName, Constants.DefaultModelsDirectory);

    private Model((Vector3[], uint[]) coordinates)
    {
        Vertices = coordinates.Item1;
        _texture = new VectorArray(DefaultModels.GetDefaultTexture(Vertices), ArrayType.TwoDimensions);
        _indices = coordinates.Item2;
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
        
        // void SetField(int index, MatchCollection matches)
        // {
        //     var value = matches[index].Value.Split(new[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        //     var doubleValue = value.Select(float.Parse).ToArray();
        //
        //     switch (index)
        //     {
        //         case 0:
        //             _vertices = doubleValue;
        //             break;
        //         case 1:
        //             _indices = value.Select(uint.Parse).ToArray();
        //             break;
        //         case 2:
        //             Size = new Vector3(doubleValue[0], doubleValue[1], doubleValue[2]);
        //             break;
        //         case 3:
        //             Position = new Vector3(doubleValue[0], doubleValue[1], doubleValue[2]);
        //             break;
        //         default:
        //             _color = new Vector4(doubleValue[0], doubleValue[1], doubleValue[2], doubleValue[3]);
        //             break;
        //     }
        // }
        //
        // var path = Path.Combine(Directory, Name + Constants.ModelExtension);
        //
        // if (!File.Exists(path))
        // {
        //     LoadError = true;
        //     return;
        // }
        //
        // var text = File.ReadAllText(path);
        // const string pattern = @"(((-?\d.\d(\s|))|\d(\s|))+\r\n)+";
        // var matches = Regex.Matches(text, pattern);
        //
        // for (var i = 0; i < matches.Count; ++i)
        //     SetField(i, matches);
        Loaded = true;
    }

    public (float[], uint[]) GetVectorArray(IGameObject obj)
    {
        (float[], uint[]) GetArrayVisibility(float[] texture) => obj.Body.Visibility switch
        {
            VisibilityType.Hidden => (GetArrays(texture, Constants.NullColor), _indices),
            VisibilityType.Collapsed => (new[] { 0f, 0, 0, -1, -1, 0, 0, 0, 0 }, new uint[] { 1 }),
            VisibilityType.Visible or _ => (GetArrays(texture, obj.Body.Color), _indices)
        };
        
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

        if (obj.Body.Visual is not (BodyVisualType.Texture or BodyVisualType.ColorAndTexture))
            return GetArrayVisibility(texture);
        
        texture = obj.Body.Visual == BodyVisualType.Texture ? _texture.GetArray() : null;
            
        if (obj.Body.Visual == BodyVisualType.Texture)
            obj.Body.Color = Constants.NullColor;

        return GetArrayVisibility(texture);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public static IModel Find(string name)
    {
        var isFound = Game.Models.TryGetValue(name, out var value);

        return isFound ? value : null;
    }
    
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Vertices = null;
            _indices = null;
            _texture = null;
            Directory = null;
            Name = null;
        }

        _disposed = true;
    }
}