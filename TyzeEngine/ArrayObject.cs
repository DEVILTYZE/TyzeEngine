using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace TyzeEngine;

public sealed class ArrayObject : IDisposable
{
    private readonly List<int> _attributeList;
    private readonly List<BufferObject> _bufferList;
    private bool _disposed;
    private int _count;
    
    public int Handle { get; }
    public bool IsEnabled { get; private set; }

    public ArrayObject()
    {
        Handle = GL.GenVertexArray();
        IsEnabled = false;
        _bufferList = new List<BufferObject>();
        _attributeList = new List<int>();
        _count = 0;
    }
    
    ~ArrayObject() => ReleaseUnmanagedResources();

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

    public void AttachBuffer(BufferObject buffer, int count)
    {
        if (!IsEnabled)
            Enable();
        
        if (!buffer.IsEnabled)
            buffer.Enable();

        _count = count;
        _bufferList.Add(buffer);
    }

    public void EnableAttribute(int locationAttribute, int count, VertexAttribPointerType type, int stride, int offset)
    {
        _attributeList.Add(locationAttribute);
        GL.EnableVertexAttribArray(locationAttribute);
        GL.VertexAttribPointer(locationAttribute, count, type, false, stride, offset);
        GL.EnableVertexAttribArray(0);
    }

    public void DisableAttributes()
    {
        foreach (var locationAttribute in _attributeList)
            GL.DisableVertexAttribArray(locationAttribute);
    }

    public void Draw() => Draw(0, _count);

    public void Draw(DrawElementsType type) => Draw(0, _count, type);
    
    public void Draw(int startIndex, int endIndex)
    {
        if (!IsEnabled)
            Enable();

        GL.DrawArrays(PrimitiveType.Triangles, startIndex, endIndex);
        Disable();
    }

    public void Draw(int startIndex, int endIndex, DrawElementsType type)
    {
        if (!IsEnabled)
            Enable();

        GL.DrawElements(PrimitiveType.Triangles, endIndex, type, startIndex);
        Disable();
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources()
    {
        if (_disposed || Handle == Constants.ErrorCode)
            return;
        
        DisableAttributes();
        Disable();
        GL.DeleteVertexArray(Handle);

        foreach (var buffer in _bufferList)
            buffer.Dispose();

        _disposed = true;
    }
}