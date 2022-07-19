using System;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL4;

namespace TyzeEngine;

public sealed class Shader : IDisposable
{
    private bool _disposed;
    
    public string Errors { get; }
    public int Handle { get; }

    public Shader(string vertexPath, string fragmentPath)
    {
        Errors = string.Empty;
        
        string vertexShaderSource, fragmentShaderSource;

        using (var sr = new StreamReader(vertexPath, Encoding.UTF8))
        {
            vertexShaderSource = sr.ReadToEnd();
        }

        using (var sr = new StreamReader(fragmentPath, Encoding.UTF8))
        {
            fragmentShaderSource = sr.ReadToEnd();
        }

        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        
        GL.CompileShader(vertexShader);
        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out var success);

        if (success == 0)
            Errors += GL.GetShaderInfoLog(vertexShader);
        
        GL.CompileShader(fragmentShader);
        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
        
        if (success == 0)
            Errors += GL.GetShaderInfoLog(vertexShader);

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vertexShader);
        GL.AttachShader(Handle, fragmentShader);
        GL.LinkProgram(Handle);
        
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);

        if (success == 0)
            Errors += GL.GetProgramInfoLog(vertexShader);
        
        GL.DetachShader(Handle, vertexShader);
        GL.DetachShader(Handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    ~Shader() => GL.DeleteProgram(Handle);
    
    public void Use() => GL.UseProgram(Handle);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) 
            return;
        
        GL.DeleteProgram(Handle);
        _disposed = true;
    }
}