using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TyzeEngine;

public sealed class Shader : IDisposable
{
    private bool _disposed;
    private readonly Dictionary<string, int> _uniformLocations;

    public string Error { get; private set; }
    public int Handle { get; }
    public bool IsEnabled { get; private set; }

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
        
        GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var countOfUniforms);
        _uniformLocations = new Dictionary<string, int>();

        for (var i = 0; i < countOfUniforms; ++i)
        {
            var key = GL.GetActiveUniform(Handle, i, out _, out _);
            var location = GL.GetUniformLocation(Handle, key);
            _uniformLocations.Add(key, location);
        }
    }

    ~Shader() => ReleaseUnmanagedResources();
    
    public void Enable()
    {
        IsEnabled = true;
        GL.UseProgram(Handle);
    }

    public void Disable()
    {
        IsEnabled = false;
        GL.UseProgram(0);
    }

    public int GetAttributeLocation(string parameterName) => GL.GetAttribLocation(Handle, parameterName);

    public void SetMatrix4(string name, Matrix4 matrix)
    {
        if (!IsEnabled)
            Enable();
        
        GL.UniformMatrix4(_uniformLocations[name], true, ref matrix);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources()
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