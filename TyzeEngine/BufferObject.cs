using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace TyzeEngine;

internal sealed class BufferObject : IDisposable
{
    private bool _disposed;
    private IntPtr _offset;
    private bool _isAllocatedMemory;
    
    internal int Handle { get; }
    internal bool IsEnabled { get; private set; }
    internal BufferTarget Type { get; }

    internal BufferObject(BufferTarget type)
    {
        Type = type;
        Handle = GL.GenBuffer();
    }

    ~BufferObject() => ReleaseUnmanagedResources();

    internal void SetData<T>(T[] data, BufferUsageHint hint) where T : struct
    {
        if (data is null || data.Length == 0)
            throw new ArgumentException("Data is null or empty.", nameof(data));

        Enable();
        GL.BufferData(Type, data.Length * Marshal.SizeOf(typeof(T)), data, hint);
        Disable();
        _isAllocatedMemory = false;
        _offset = IntPtr.Zero;
    }

    internal void AllocMemory(int size, BufferUsageHint hint)
    {
        if (size <= 0)
            throw new ArgumentException("Size less then zero or equal.");
        
        Enable();
        GL.BufferData(Type, size, IntPtr.Zero, hint);
        Disable();
        _isAllocatedMemory = true;
    }

    internal void SetSubData<T>(T[] data) where T : struct
    {
        if (data is null || data.Length == 0)
            throw new ArgumentException("Data is null or empty.", nameof(data));
        if (!_isAllocatedMemory)
            throw new Exception("Memory is not allocated.");
        
        Enable();
        var size = data.Length * Marshal.SizeOf(typeof(T));
        GL.BufferSubData(Type, _offset, size, data);
        _offset += size;

        Disable();
    }

    internal void Enable()
    {
        GL.BindBuffer(Type, Handle);
        IsEnabled = true;
    }

    internal void Disable()
    {
        GL.BindBuffer(Type, 0);
        IsEnabled = false;
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
        
        Disable();
        GL.DeleteBuffer(Handle);
        _disposed = true;
    }
}