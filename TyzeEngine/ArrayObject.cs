using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace TyzeEngine;

public enum AttributeDataType
{
    Float = VertexAttribPointerType.Float
}

public enum ElementType
{
    Uint = DrawElementsType.UnsignedInt
}

public sealed class ArrayObject : IDisposable
{
    private readonly List<int> _attributeList;
    private readonly List<BufferObject> _bufferList;
    private bool _disposed;
    
    public int Handle { get; }
    public bool IsEnabled { get; private set; }

    public ArrayObject()
    {
        Handle = GL.GenVertexArray();
        IsEnabled = false;
        _bufferList = new List<BufferObject>();
        _attributeList = new List<int>();
    }
    
    ~ArrayObject() => Dispose(false);

    public void Enable()
    {
        GL.BindVertexArray(Handle);
        IsEnabled = true;
    }

    public void Disable()
    {
        GL.BindVertexArray(0);
        IsEnabled = false;
    }

    public void AttachBuffer(BufferObject buffer)
    {
        if (!IsEnabled)
            Enable();
        
        if (!buffer.IsEnabled)
            buffer.Enable();
        
        _bufferList.Add(buffer);
    }

    public void EnableAttribute(int locationAttribute, int count, AttributeDataType type, int stride, int offset)
    {
        _attributeList.Add(locationAttribute);
        GL.EnableVertexAttribArray(locationAttribute);
        GL.VertexAttribPointer(locationAttribute, count, (VertexAttribPointerType)type, false, 
            stride, offset);
    }

    public void DisableAttributes()
    {
        foreach (var locationAttribute in _attributeList)
            GL.DisableVertexAttribArray(locationAttribute);
    }

    public void Draw(int startIndex, int endIndex)
    {
        if (!IsEnabled)
            Enable();

        GL.DrawArrays(PrimitiveType.Triangles, startIndex, endIndex);
    }

    public void Draw(int startIndex, int endIndex, ElementType type)
    {
        if (!IsEnabled)
            Enable();

        GL.DrawElements(PrimitiveType.Triangles, endIndex, (DrawElementsType)type, startIndex);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed || Handle == ConstHelper.ErrorCodeBuffer)
            return;
        
        Disable();
        GL.DeleteVertexArray(Handle);

        foreach (var buffer in _bufferList)
            buffer.Dispose();
    }
}