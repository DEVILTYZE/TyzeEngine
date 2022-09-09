using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace TyzeEngine;

internal sealed class BufferObject : IDisposable
{
    private bool _disposed;
    
    internal int Handle { get; }
    internal bool IsEnabled { get; private set; }
    internal BufferTarget Type { get; }

    internal BufferObject(BufferTarget type)
    {
        Type = type;
        Handle = GL.GenBuffer();
        IsEnabled = false;
    }

    ~BufferObject() => ReleaseUnmanagedResources();

    internal void SetData<T>(T[] data, BufferUsageHint hint) where T : struct
    {
        if (data is null || data.Length == 0)
            throw new ArgumentException("Data is null or empty.", nameof(data));

        Enable();
        GL.BufferData(Type, data.Length * Marshal.SizeOf(typeof(T)), data, hint);
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