using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public class Model : IModel
{
    private bool _disposed;
    private float[] _array;

    protected IVectorArray Texture = new VectorArray();
    protected List<uint> LineIndices = new();

    ArrayObject IModel.ArrayObject { get; set; }
    
    public UId Id { get; set; } = new();
    public IReadOnlyList<Vector3> Vertices { get; protected set; }
    public IEnumerable<Vector2> Vertices2D => Vertices.Select(vertex => vertex.Xy).ToArray();
    public IReadOnlyList<uint> Indices { get; protected set; }
    public IEnumerable<float> TextureCoordinates => Texture.GetArray();
    public IReadOnlyList<Vector3> Normals { get; protected set; }
    public string Directory { get; private set; }
    public string Name { get; private set; }
    public bool Loaded => _array is not null;

    public Model(string name, string directory = Constants.ModelsDirectory)
    {
        Directory = directory;
        Name = name;
    }

    protected Model()
    {
    }
    
    ~Model() => Dispose(false);

    public void LoadFromFile()
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

        // foreach (var str in text)
        //     ParseWavefrontString(str);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    void IModel.Load()
    {
        var arrayObject = ((IModel)this).ArrayObject;
        
        if (arrayObject is not null)
            return;
        
        var shader = Game.Shaders[BodyVisualType.Object];
        shader.Enable();
        arrayObject = new ArrayObject();
        ((IModel)this).ArrayObject = arrayObject;
        
        // Создание буферa для векторного представления.
        var arrayBuffer = new BufferObject(BufferTarget.ArrayBuffer);
        arrayBuffer.SetData(_array, BufferUsageHint.StaticDraw);
        arrayObject.AttachBuffer(arrayBuffer);
        
        // Получение индексов для трёх атрибутов (позиция, текстура и цвет).
        const int stride = Constants.Vector3Stride * 2 + Constants.Vector2Stride;
        var position = shader.GetAttributeLocation("aPosition");
        var normal = shader.GetAttributeLocation("inNormal");
        var texture = shader.GetAttributeLocation("inTexture");
        arrayBuffer.Enable();
        arrayObject.EnableAttribute(position, Constants.Vector3Length, VertexAttribPointerType.Float, 
            stride, 0);
        arrayObject.EnableAttribute(normal, Constants.Vector3Length, VertexAttribPointerType.Float, 
            stride, Constants.Vector3Stride);
        arrayObject.EnableAttribute(texture, Constants.Vector2Length, VertexAttribPointerType.Float, 
            stride, Constants.Vector3Stride * 2);
        arrayBuffer.Disable();
        
        // Создание буфера для Element object.
        var elementBuffer = new BufferObject(BufferTarget.ElementArrayBuffer);
        elementBuffer.SetData(Indices.ToArray(), BufferUsageHint.StaticDraw);
        arrayObject.AttachBuffer(elementBuffer);
        arrayObject.Disable();
        shader.Disable();
    }

    /// <summary>
    /// Ищет модель по имени среди всех добавленных в игру моделей.
    /// </summary>
    /// <param name="name">Имя модели.</param>
    /// <returns>Объект модели, приведённый к типу IModel.</returns>
    /// <exception cref="ArgumentNullException">Имя равно null.</exception>
    /// <exception cref="Exception">Модель не найдена.</exception>
    public static IModel Find([NotNull] string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name), "Name was null.");
        
        var isFound = Game.Models.TryGetValue(name, out var value);

        return isFound ? value : throw new Exception("Model not found.");
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

    protected void SetModel()
    {
        if (Vertices is null)
            throw new NullReferenceException("Vertices list is null.");
        if (Normals is null)
            throw new NullReferenceException("Normals list is null.");
        if (Texture is null)
            throw new NullReferenceException("Texture coordinates list is null.");
        if (Indices is null)
            throw new NullReferenceException("Indices list is null.");
        
        if (Vertices.Count == 0 || Normals.Count == 0 || Texture.Length == 0)
            throw new Exception("Vertices count is zero.");
        if (Normals.Count == 0)
            throw new Exception("Normals count is zero.");
        if (Texture.Length == 0)
            throw new Exception("Texture coordinates count is zero.");
        if (Indices.Count == 0)
            throw new Exception("Indices count is zero.");
        
        Normals = Normals.Select(Vector3.NormalizeFast).ToList();
        MixArrays();
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        ReleaseUnmanagedResources();
        
        if (disposing)
        {
            Vertices = null;
            Normals = null;
            Indices = null;
            Texture = null;
            Directory = null;
            Name = null;
        }

        _disposed = true;
    }
    
    private void ReleaseUnmanagedResources() => ((IModel)this).ArrayObject?.Dispose();

    private void MixArrays()
    {
        var result = new List<float>();

        for (var i = 0; i < Vertices.Count; ++i)
        {
            result.AddRange(new[] { Vertices[i].X, Vertices[i].Y, Vertices[i].Z });
            result.AddRange(new[] { Normals[i].X, Normals[i].Y, Normals[i].Z });
            result.AddRange(Texture[i]);
        }

        _array = result.ToArray();
    }

    // private void ParseWavefrontString(string str)
    // {
    //     var parts = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    //     const string vertex = "v";
    //     const string texture = "vt";
    //     const string normal = "vn";
    //     const string space = "vp";
    //     const string index = "f";
    //
    //     switch (parts[0])
    //     {
    //         case vertex:
    //             var value = parts[1..].Select(float.Parse).ToArray();
    //             _vertices.Add(new Vector3(value[0], value[1], value[2]));
    //             break;
    //         case texture:
    //             value = parts[1..].Select(float.Parse).ToArray();
    //             _texture.Add(value[0], value[1]);
    //             break;
    //         case index:
    //             var indexValue = parts[1..]
    //                 .Select(localStr => localStr.Split('/', StringSplitOptions.RemoveEmptyEntries))
    //                 .SelectMany(localStr => localStr).Select(uint.Parse);
    //             _indices.AddRange(indexValue);
    //             break;
    //         case normal: // На будущее...
    //             break;
    //         case space:
    //             break;
    //     }
    // }
}