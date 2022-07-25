using System;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TyzeEngine;

public sealed class Shader : IDisposable
{
    private bool _disposed;
    
    public string Error { get; private set; }
    public int Handle { get; }

    public Shader(string vertexPath, string fragmentPath)
    {
        Error = string.Empty;

        var vertexShader = CreateShader(vertexPath, ShaderType.VertexShader);
        var fragmentShader = CreateShader(fragmentPath, ShaderType.FragmentShader);

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vertexShader);
        GL.AttachShader(Handle, fragmentShader);
        GL.LinkProgram(Handle);
        
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out var errorCode);

        if (errorCode != (int)All.True)
        {
            Error = GL.GetProgramInfoLog(vertexShader);
            throw new Exception($"Shader program link error: {Error}");
        }
        
        GL.DetachShader(Handle, vertexShader);
        GL.DetachShader(Handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    ~Shader() => GL.DeleteProgram(Handle);
    
    public void Enable() => GL.UseProgram(Handle);

    public void Disable() => GL.UseProgram(0);

    public int GetAttributeLocation(string parameterName) => GL.GetAttribLocation(Handle, parameterName);

    public void SetUniform(string uniformName, Vector4 vector)
    {
        var location = GL.GetUniformLocation(Handle, uniformName);
        GL.Uniform4(location, vector);
    }

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

    private int CreateShader(string path, ShaderType type)
    {
        var shaderSource = File.ReadAllText(path, Encoding.UTF8);
        var shader = GL.CreateShader(type);
        GL.ShaderSource(shader, shaderSource);
        
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out var errorCode);

        if (errorCode == (int)All.True) 
            return shader;
        
        Error = GL.GetShaderInfoLog(shader);
        throw new Exception($"Shader compile error: {Error}");
    }
}