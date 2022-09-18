using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

namespace TyzeEngine.Objects;

public class Mesh : IMesh
{
    private bool _disposed;
    private float[] _array;
    private ArrayObject _arrayObject;
    
    protected List<uint> LineIndices = new();
    
    internal IVectorArray Texture = new VectorArray();

    public IReadOnlyList<Vector3> Vertices { get; internal set; }
    public IReadOnlyList<Vector2> Vertices2D => Vertices.Select(vector => vector.Xy).ToList();
    public IReadOnlyList<uint> Indices { get; internal set; }
    public IReadOnlyList<float> TextureCoordinates => Texture.GetArray();
    public IReadOnlyList<Vector3> Normals { get; internal set; }
    public List<Texture> Textures { get; } = new();

    ~Mesh() => Dispose(false);
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    void IMesh.Load()
    {
        var shader = Game.Shaders[BodyVisualType.Object];
        shader.Enable();
        _arrayObject = new ArrayObject();
        
        // Создание буферa для векторного представления.
        var arrayBuffer = new BufferObject(BufferTarget.ArrayBuffer);
        arrayBuffer.SetData(_array, BufferUsageHint.StaticDraw);
        _arrayObject.AttachBuffer(arrayBuffer);
        
        // Получение индексов для трёх атрибутов (позиция, текстура и цвет).
        const int stride = Constants.Vector3Stride * 2 + Constants.Vector2Stride;
        var position = shader.GetAttributeLocation("aPosition");
        var normal = shader.GetAttributeLocation("inNormal");
        var texture = shader.GetAttributeLocation("inTexture");
        arrayBuffer.Enable();
        _arrayObject.EnableAttribute(position, Constants.Vector3Length, VertexAttribPointerType.Float, 
            stride, 0);
        _arrayObject.EnableAttribute(normal, Constants.Vector3Length, VertexAttribPointerType.Float, 
            stride, Constants.Vector3Stride);
        _arrayObject.EnableAttribute(texture, Constants.Vector2Length, VertexAttribPointerType.Float, 
            stride, Constants.Vector3Stride * 2);
        arrayBuffer.Disable();
        
        // Создание буфера для Element object.
        var elementBuffer = new BufferObject(BufferTarget.ElementArrayBuffer);
        elementBuffer.SetData(Indices.ToArray(), BufferUsageHint.StaticDraw);
        _arrayObject.AttachBuffer(elementBuffer);
        _arrayObject.Disable();
        shader.Disable();
    }

    void IMesh.Draw(PrimitiveType type)
    {
        _arrayObject.Enable();
        _arrayObject.Draw(type, 0, Indices.Count, DrawElementsType.UnsignedInt);
        _arrayObject.Disable();
    }

    void IMesh.SetMesh()
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

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Vertices = null;
            Indices = null;
            Normals = null;
        }
        
        ReleaseUnmanagedResources();
        _disposed = true;
    }

    private void ReleaseUnmanagedResources() => _arrayObject.Dispose();
}