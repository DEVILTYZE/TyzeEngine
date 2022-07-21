using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace TyzeEngine;

public enum BufferType
{
    ArrayBuffer = BufferTarget.ArrayBuffer,
    ElementBuffer = BufferTarget.ElementArrayBuffer
}

public enum BufferHint
{
    StaticDraw = BufferUsageHint.StaticDraw,
    DynamicDraw = BufferUsageHint.DynamicDraw,
    StreamDraw = BufferUsageHint.StreamDraw
}

public sealed class BufferObject : IDisposable
{
    private readonly BufferTarget _type;
    private bool _disposed;
    
    public int Handle { get; }
    public bool IsEnabled { get; private set; }

    public BufferObject(BufferType type)
    {
        _type = (BufferTarget)type;
        Handle = GL.GenBuffer();
        IsEnabled = false;
    }

    ~BufferObject() => Dispose(false);

    public void SetData<T>(T[] data, BufferHint hint) where T : struct
    {
        if (data is null || data.Length == 0)
            throw new ArgumentException("Data is null or empty.", nameof(data));
        
        Enable();
        GL.BufferData(_type, data.Length * Marshal.SizeOf(typeof(T)), data, (BufferUsageHint)hint);
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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed || Handle == ConstHelper.ErrorCodeBuffer)
            return;
        
        Disable();
        GL.DeleteBuffer(Handle);
    }
}