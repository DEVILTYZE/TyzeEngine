using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace TyzeEngine;

internal sealed class ArrayObject : IDisposable
{
    private readonly List<int> _attributeList;
    private readonly List<BufferObject> _bufferList;
    private bool _disposed;
    private int _count;

    internal int Handle { get; }
    internal bool IsEnabled { get; private set; }

    internal ArrayObject()
    {
        Handle = GL.GenVertexArray();
        IsEnabled = false;
        _bufferList = new List<BufferObject>();
        _attributeList = new List<int>();
        _count = 0;
    }
    
    ~ArrayObject() => ReleaseUnmanagedResources();

    internal void Enable()
    {
        GL.BindVertexArray(Handle);
        IsEnabled = true;
    }

    internal void Disable()
    {
        GL.BindVertexArray(0);
        IsEnabled = false;
    }

    internal void AttachBuffer(BufferObject buffer, int count)
    {
        if (!IsEnabled)
            Enable();
        
        if (!buffer.IsEnabled)
            buffer.Enable();

        _count = count;
        _bufferList.Add(buffer);
    }

    internal void EnableAttribute(int locationAttribute, int count, VertexAttribPointerType type, int stride, int offset)
    {
        _attributeList.Add(locationAttribute);
        GL.EnableVertexAttribArray(locationAttribute);
        GL.VertexAttribPointer(locationAttribute, count, type, false, stride, offset);
        GL.EnableVertexAttribArray(0);
    }

    internal void DisableAttributes()
    {
        foreach (var locationAttribute in _attributeList)
            GL.DisableVertexAttribArray(locationAttribute);
    }

    internal void Draw(PrimitiveType primitive) => Draw(primitive, 0, _count);

    internal void Draw(PrimitiveType primitive, DrawElementsType type) => Draw(primitive, 0, _count, type);
    
    internal void Draw(PrimitiveType primitive, int startIndex, int endIndex)
    {
        if (!IsEnabled)
            Enable();

        GL.DrawArrays(primitive, startIndex, endIndex);
        Disable();
    }

    internal void Draw(PrimitiveType primitive, int startIndex, int endIndex, DrawElementsType type)
    {
        if (!IsEnabled)
            Enable();

        GL.DrawElements(primitive, endIndex, type, startIndex);
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