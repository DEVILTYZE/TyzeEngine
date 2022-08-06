using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public class Model : IModel, IDisposable
{
    private bool _disposed;
    private float[] _vertices;
    private uint[] _indices;
    private IVectorArray _texture;

    public Uid Id { get; }
    public string Directory { get; }
    public string Name { get; }
    public bool Loaded { get; private set; }

    public static IModel Point => new Model(DefaultModels.GetPoint()) { Loaded = true };
    public static IModel Triangle => new Model(DefaultModels.GetTriangle()) { Loaded = true };
    public static IModel Rectangle => new Model(DefaultModels.GetRectangle()) { Loaded = true };
    public static IModel Circle => new Model(DefaultModels.GetCircle()) { Loaded = true };
    // public static IModel Cube => new Model(Constants.TriangleModelName, Constants.DefaultModelsDirectory);

    private Model((float[], uint[]) coordinates)
    {
        Id = new Uid();
        _vertices = coordinates.Item1;
        _texture = null;
        _indices = coordinates.Item2;
    }
    
    public Model(string name, string directory = Constants.ModelsDirectory)
    {
        Directory = directory;
        Name = name;
    }
    
    ~Model() => Dispose(false);

    public void Load()
    {
        //     void SetField(int index, MatchCollection matches)
        //     {
        //         var value = matches[index].Value.Split(new[] { " ", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        //         var doubleValue = value.Select(float.Parse).ToArray();
        //     
        //         switch (index)
        //         {
        //             case 0:
        //                 _vertices = doubleValue;
        //                 break;
        //             case 1:
        //                 _indices = value.Select(uint.Parse).ToArray();
        //                 break;
        //             case 2:
        //                 Size = new Vector3(doubleValue[0], doubleValue[1], doubleValue[2]);
        //                 break;
        //             case 3:
        //                 Position = new Vector3(doubleValue[0], doubleValue[1], doubleValue[2]);
        //                 break;
        //             default:
        //                 _color = new Vector4(doubleValue[0], doubleValue[1], doubleValue[2], doubleValue[3]);
        //                 break;
        //         }
        //     }
        //     
        //     var path = Path.Combine(Directory, Name + Constants.ModelExtension);
        //
        //     if (!File.Exists(path))
        //     {
        //         LoadError = true;
        //         return;
        //     }
        //
        //     var text = File.ReadAllText(path);
        //     const string pattern = @"(((-?\d.\d(\s|))|\d(\s|))+\r\n)+";
        //     var matches = Regex.Matches(text, pattern);
        //     
        //     for (var i = 0; i < matches.Count; ++i)
        //         SetField(i, matches);
        Loaded = true;
    }

    public (float[], uint[]) GetVectorArray(IGameObject obj)
    {
        _texture = obj.Body.HasTexture 
            ? new VectorArray(DefaultModels.GetDefaultTexture(_vertices), ArrayType.TwoDimensions) 
            : null;
        
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
        var colorArray = new[] { obj.Body.Color.X, obj.Body.Color.Y, obj.Body.Color.Z, obj.Body.Color.W };

        return obj.Body.Visibility switch
        {
            VisibilityType.Hidden => (GetArrays(texture, new[] { 0f, 0, 0, 0 }, false), _indices),
            VisibilityType.Collapsed => (new[] { 0f, 0, 0, -1, -1, 0, 0, 0, 0 }, new uint[] { 1 }),
            VisibilityType.Visible or _ => _texture is null
                ? (GetArrays(texture, colorArray, false), _indices)
                : (GetArrays(_texture.GetArray(), colorArray, true), _indices)
        };
    }

    public float GetVolume(Vector3 scale)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // something...
        }

        _vertices = null;
        _indices = null;
        _disposed = true;
    }
}