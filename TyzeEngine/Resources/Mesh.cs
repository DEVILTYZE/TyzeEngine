using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine.Resources;

public class Mesh : IMesh
{
    private bool _disposed;
    private ArrayObject _arrayObject;

    private int Size => GetSize(Vertices.Count + Normals.Count, typeof(Vector3)) + GetSize(
        TextureCoordinates.Count, typeof(Vector2));

    public INode Node { get; }
    public IReadOnlyList<Vector3> Vertices { get; internal set; }
    public IReadOnlyList<Vector2> Vertices2D => Vertices.Select(vertex => vertex.Xy).ToList();
    public IReadOnlyList<Vector3> VerticesTransformed =>
        Vertices.Select(vertex => (Node.Transform * new Vector4(vertex)).Xyz).ToList();
    public IReadOnlyList<Vector2> VerticesTransformed2D =>
        Vertices.Select(vertex => (Node.Transform * new Vector4(vertex.Xy)).Xy).ToList();
    public IReadOnlyList<uint> Indices { get; internal set; }
    public IReadOnlyList<Vector2> TextureCoordinates { get; internal set; }
    public IReadOnlyList<Vector3> Normals { get; internal set; }
    public List<Texture> Textures { get; internal set; } = new();
    public MeshBody MeshBody { get; private set; }

    public Mesh(INode node) => Node = node;
    
    ~Mesh() => Dispose(false);

    public IMesh Clone(IMesh obj = null)
    {
        var mesh = (Mesh)obj ?? new Mesh(Node);
        mesh.Vertices = new List<Vector3>(Vertices);
        mesh.Indices = new List<uint>(Indices);
        mesh.TextureCoordinates = new List<Vector2>(TextureCoordinates);
        mesh.Normals = new List<Vector3>(Normals);
        mesh.Textures = new List<Texture>(Textures);

        return mesh;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public CollisionPoints TestCollision(ITransform transform, IMesh mesh, ITransform bodyTransform) =>
        MeshBody.TestCollision(transform, mesh.MeshBody, bodyTransform);

    void IMesh.Load()
    {
        var shader = Game.Shaders[BodyVisualType.Object];
        shader.Enable();
        _arrayObject = new ArrayObject();
        _arrayObject.Enable();
        
        // Создание буферa для векторного представления.
        var arrayBuffer = new BufferObject(BufferTarget.ArrayBuffer);
        //arrayBuffer.SetData(_array, BufferUsageHint.StaticDraw);
        arrayBuffer.AllocMemory(Size, BufferUsageHint.StaticDraw);
        arrayBuffer.SetSubData(Vertices.SelectMany(vector => new[] { vector.X, vector.Y, vector.Z }).ToArray());
        arrayBuffer.SetSubData(Normals.SelectMany(vector => new[] { vector.X, vector.Y, vector.Z }).ToArray());
        arrayBuffer.SetSubData(TextureCoordinates.SelectMany(vector => new[] { vector.X, vector.Y }).ToArray());
        _arrayObject.AttachBuffer(arrayBuffer);
        
        // Получение индексов для трёх атрибутов (позиция, текстура и цвет).
        var position = shader.GetAttributeLocation("aPosition");
        var normal = shader.GetAttributeLocation("inNormal");
        var textureIndex = shader.GetAttributeLocation("inTexture");
        
        // Включение атрибутов для array object.
        arrayBuffer.Enable();
        _arrayObject.EnableAttribute(position, Constants.Vector3Length, VertexAttribPointerType.Float, 
            Constants.Vector3Stride, 0);
        _arrayObject.EnableAttribute(normal, Constants.Vector3Length, VertexAttribPointerType.Float, 
            Constants.Vector3Stride, GetSize(Vertices.Count, typeof(Vector3)));
        _arrayObject.EnableAttribute(textureIndex, Constants.Vector2Length, VertexAttribPointerType.Float, 
            Constants.Vector2Stride, GetSize(Vertices.Count + Normals.Count, typeof(Vector3)));
        arrayBuffer.Disable();
        
        // Создание буфера для Element object.
        var elementBuffer = new BufferObject(BufferTarget.ElementArrayBuffer);
        elementBuffer.SetData(Indices.ToArray(), BufferUsageHint.StaticDraw);
        _arrayObject.AttachBuffer(elementBuffer);
        _arrayObject.Disable();
        shader.Disable();
    }

    void IMesh.Draw(Shader shader)
    {
        for (var i = 0; i < Textures.Count; ++i)
        {
            Textures[i].Unit = TextureUnit.Texture0 + i;
            Textures[i].Activate();
            shader.SetTexture("material", i, Textures[i]);
            Textures[i].Enable();
        }
        
        if (Textures.Count > 0)
            Textures[0].Activate();

        _arrayObject.Enable();
        _arrayObject.Draw(PrimitiveType.Triangles, 0, Indices.Count, DrawElementsType.UnsignedInt);
        _arrayObject.Disable();
        Textures.ForEach(texture => texture.Disable());
    }

    void IMesh.DrawLines()
    {
        _arrayObject.Enable();
        _arrayObject.Draw(PrimitiveType.LineLoop, 0, Indices.Count, DrawElementsType.UnsignedInt);
        _arrayObject.Disable();
    }

    void IMesh.SetMesh(int dimension)
    {
        if (Vertices is null)
            throw new NullReferenceException("Vertices list is null.");
        if (Normals is null)
            throw new NullReferenceException("Normals list is null.");
        if (TextureCoordinates is null)
            throw new NullReferenceException("Texture coordinates list is null.");
        if (Indices is null)
            throw new NullReferenceException("Indices list is null.");
        
        if (Vertices.Count == 0)
            throw new Exception("Vertices count is zero.");
        if (Normals.Count == 0)
            throw new Exception("Normals count is zero.");
        if (TextureCoordinates.Count == 0)
            throw new Exception("Texture coordinates count is zero.");
        if (Indices.Count == 0)
            throw new Exception("Indices count is zero.");
        
        Normals = Normals.Select(Vector3.NormalizeFast).ToList();
        MeshBody = new MeshBody(new Static(false), this, dimension);
    }

    private static int GetSize(int count, Type type)
    {
        if (!type.IsValueType)
            throw new Exception("Type is not value type.");
        
        return count * Marshal.SizeOf(type);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Vertices = null;
            Indices = null;
            Normals = null;
            TextureCoordinates = null;
        }
        
        _arrayObject.Dispose();
        _disposed = true;
    }
}