using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Assimp;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;
using TyzeEngine.Resources;
using AssimpNode = Assimp.Node;
using AssimpMesh = Assimp.Mesh;
using AssimpMaterial = Assimp.Material;
using Mesh = TyzeEngine.Resources.Mesh;
using Node = TyzeEngine.Resources.Node;

namespace TyzeEngine.GameStructure;

public class Model : IModel
{
    private bool _disposed;

    public UID Id { get; set; } = new();
    public INode RootNode { get; private set; } = new Node(null);
    public string Directory { get; private set; }
    public string Name { get; private set; }
    public bool Loaded => RootNode.HasMeshes || RootNode.HasChildren;

    public Model(string name, string directory = Constants.ModelsDirectory)
    {
        Directory = directory;
        Name = name;
    }

    protected Model()
    {
    }

    ~Model() => Dispose(false);

    public CollisionPoints TestCollision(ITransform transform, IModel model, ITransform bodyTransform) =>
        RootNode.TestCollision(transform, model.RootNode, bodyTransform);

    public void LoadFromFile()
    {
        if (Loaded)
            return;

        var path = Directory.EndsWith('\\') || Directory.EndsWith('/') ? Directory + Name : Directory + '\\' + Name;

        if (!File.Exists(path))
            throw new FileNotFoundException("Model file not found.", Directory + Name);

        using var importer = new AssimpContext();
        var scene = importer.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

        if (scene is null || scene.SceneFlags == SceneFlags.Incomplete || scene.RootNode is null)
            throw new Exception("Import model error.");

        ProcessNode(scene.RootNode, scene, RootNode);
    }

    public IModel Clone(IModel obj = null)
    {
        var model = (Model)obj ?? new Model();
        model.RootNode = RootNode.Clone();

        return model;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    void IModel.Load() => RootNode.Load();
    
    void IModel.Draw(Shader shader)
    {
        if (!Loaded)
            throw new Exception("Model not loaded.");
        
        RootNode.Draw(shader);
    }

    void IModel.DrawLines()
    {
        if (!Loaded)
            throw new Exception("Model not loaded.");
        
        RootNode.DrawLines();
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
    
    internal static List<Vector2> GetDefaultTexture(IReadOnlyList<Vector3> vertices)
    {
        var result = new List<Vector2>(vertices.Count);

        for (var i = 0; i < vertices.Count; ++i)
            result.Add(new Vector2((vertices[i].X + 1) / 2, (vertices[i].Y + 1) / 2));
        
        return result;
    }
    
    void IGameResource.Remove()
    {
    }

    private void ProcessNode(AssimpNode aiNode, Assimp.Scene aiScene, INode modelNode)
    {
        for (var i = 0; i < aiNode.MeshCount; ++i)
        {
            var mesh = aiScene.Meshes[aiNode.MeshIndices[i]];
            modelNode.Meshes.Add(ProcessMesh(mesh, aiScene, modelNode));
        }
        
        for (var i = 0; i < aiNode.ChildCount; ++i)
        {
            var node = new Node(modelNode);
            modelNode.Children.Add(node);
            ProcessNode(aiNode.Children[i], aiScene, node);
        }
    }

    private IMesh ProcessMesh(AssimpMesh mesh, Assimp.Scene scene, INode modelNode)
    {
        if (!mesh.HasNormals)
            throw new Exception("Mesh doesn't has normals.");
        
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var textureCoordinates = new List<Vector2>();
        var textures = new List<Texture>();
        var indices = new List<uint>();
        
        var currentTexture = mesh.TextureCoordinateChannels is null
            ? new List<Vector3D>(0)
            : mesh.TextureCoordinateChannels[0];
        mesh.Faces.SelectMany(face => face.Indices).ToList().ForEach(index => indices.Add((uint)index));

        for (var i = 0; i < mesh.VertexCount; ++i)
        {
            vertices.Add(new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z));
            normals.Add(new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z));
            
            if (i < currentTexture.Count)
                textureCoordinates.Add(new Vector2(currentTexture[i].X, currentTexture[i].Y));
        }

        if (mesh.MaterialIndex >= 0)
        {
            var material = scene.Materials[mesh.MaterialIndex];
            var diffuseMaps = LoadMaterialTextures(material, TextureType.Diffuse);
            var specularMaps = LoadMaterialTextures(material, TextureType.Specular);
            textures.AddRange(diffuseMaps.Concat(specularMaps));
        }
        
        IMesh modelMesh = new Mesh(modelNode)
        {
            Vertices = vertices,
            Normals = normals,
            TextureCoordinates = textureCoordinates,
            Indices = indices,
            Textures = textures
        };
        modelMesh.SetMesh(3);
        return modelMesh;
    }

    private IEnumerable<Texture> LoadMaterialTextures(AssimpMaterial material, TextureType type)
    {
        var count = material.GetMaterialTextureCount(type);
        var list = new List<Texture>(count);

        for (var i = 0; i < count; ++i)
        {
            var isGotTexture = material.GetMaterialTexture(type, i, out var texture);
            
            if (!isGotTexture)
                continue;

            var filePath = Directory + texture.FilePath;
            
            if (Resource.IsExists(filePath, out var name))
                list.Add((Texture)Resource.Find(name));
            else
            {
                var newResource = Resource.CreateObject(filePath);
                Game.Add(name, newResource);
                list.Add((Texture)newResource);
            }
        }

        return list;
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Directory = null;
            Name = null;
        }

        RootNode.Dispose();
        _disposed = true;
    }
}