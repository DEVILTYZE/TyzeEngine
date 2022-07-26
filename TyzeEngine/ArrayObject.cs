﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL4;

namespace TyzeEngine;

internal sealed class ArrayObject : IDisposable
{
    private readonly List<int> _attributeList;
    private bool _disposed;

    internal SortedList<BufferTarget, BufferObject> Buffers { get; }
    internal int Handle { get; }
    internal bool IsEnabled { get; private set; }

    internal ArrayObject()
    {
        Handle = GL.GenVertexArray();
        IsEnabled = false;
        Buffers = new SortedList<BufferTarget, BufferObject>();
        _attributeList = new List<int>();
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

    internal void AttachBuffer(BufferObject buffer)
    {
        if (!IsEnabled)
            throw new Exception("Array object is not enabled.");
        
        if (!buffer.IsEnabled)
            buffer.Enable();

        Buffers.Add(buffer.Type, buffer);
    }

    internal void EnableAttribute(int locationAttribute, int size, VertexAttribPointerType type, int stride, int offset)
    {
        _attributeList.Add(locationAttribute);
        GL.EnableVertexAttribArray(locationAttribute);
        GL.VertexAttribPointer(locationAttribute, size, type, false, stride, offset);
        GL.EnableVertexAttribArray(0);
    }

    internal void DisableAttributes()
    {
        _attributeList.ForEach(GL.DisableVertexAttribArray);
        _attributeList.Clear();
    }
    
    internal void Draw(PrimitiveType primitive, int first, int count)
    {
        if (!IsEnabled)
            throw new Exception("Array object is not enabled.");

        GL.DrawArrays(primitive, first, count);
        Disable();
    }

    internal void Draw(PrimitiveType primitive, int startIndex, int count, DrawElementsType type)
    {
        if (!IsEnabled)
            throw new Exception("Array object is not enabled.");

        GL.DrawElements(primitive, count, type, startIndex);
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
        Buffers.ToList().ForEach(pair => pair.Value?.Dispose());
        _disposed = true;
    }
}