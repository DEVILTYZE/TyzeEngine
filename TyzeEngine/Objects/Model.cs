using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Assimp;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace TyzeEngine.Objects;

public class Model : IModel
{
    private bool _disposed;
    private List<IMesh> _meshes = new();

    public UId Id { get; set; } = new();
    public IReadOnlyList<IMesh> Meshes { get => _meshes; protected set => _meshes = value.ToList(); }
    public string Directory { get; private set; }
    public string Name { get; private set; }
    public bool Loaded => Meshes is not null && Meshes.Count > 0;

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

        var path = Directory + Name;

        if (!File.Exists(path))
            throw new FileNotFoundException("Model file not found.", Directory + Name);

        using var importer = new AssimpContext();
        var scene = importer.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

        if (scene is null || scene.SceneFlags == SceneFlags.Incomplete || scene.RootNode is null)
            throw new Exception($"Import model error.");
            
        ProcessNode(scene.RootNode, scene);
        _meshes.ForEach(mesh => mesh.SetMesh());
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    void IModel.Load() => _meshes.ForEach(mesh => mesh.Load());
    
    void IModel.Draw(PrimitiveType type) => _meshes.ForEach(mesh => mesh.Draw(type));

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

    private void ProcessNode(Node node, Scene scene)
    {
        for (var i = 0; i < node.MeshCount; ++i)
        {
            var mesh = scene.Meshes[node.MeshIndices[i]];
            _meshes.Add(ProcessMesh(mesh));
        }
        
        for (var i = 0; i < node.ChildCount; ++i)
            ProcessNode(node.Children[i], scene);
    }

    private static IMesh ProcessMesh(Assimp.Mesh mesh)
    {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var texture = new VectorArray();
        var currentTexture = mesh.TextureCoordinateChannels is null
            ? new List<Vector3D>(0)
            : mesh.TextureCoordinateChannels[0];
        var indices = new List<uint>();
        mesh.Faces.SelectMany(face => face.Indices).ToList().ForEach(index => indices.Add((uint)index));

        for (var i = 0; i < mesh.VertexCount; ++i)
        {
            vertices.Add(new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z));
            normals.Add(new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z));
            
            if (i < currentTexture.Count)
                texture.Add(currentTexture[i].X, currentTexture[i].Y);
        }
        
        return new Mesh
        {
            Vertices = vertices,
            Normals = normals,
            Texture = texture,
            Indices = indices
        };
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Meshes.ToList().ForEach(mesh => mesh.Dispose());
            Directory = null;
            Name = null;
        }

        _disposed = true;
    }
}