using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace TyzeEngine;

public sealed class BufferObject : IDisposable
{
    private readonly BufferTarget _type;
    private bool _disposed;
    
    public int Handle { get; }
    public bool IsEnabled { get; private set; }

    public BufferObject(BufferTarget type)
    {
        _type = type;
        Handle = GL.GenBuffer();
        IsEnabled = false;
    }

    ~BufferObject() => ReleaseUnmanagedResources();

    public void SetData<T>(T[] data, BufferUsageHint hint) where T : struct
    {
        if (data is null || data.Length == 0)
            throw new ArgumentException("Data is null or empty.", nameof(data));

        Enable();
        GL.BufferData(_type, data.Length * Marshal.SizeOf(typeof(T)), data, hint);
        Disable();
    }

    public void Enable()
    {
        GL.BindBuffer(_type, Handle);
        IsEnabled = true;
    }

    public void Disable()
    {
        GL.BindBuffer(_type, 0);
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