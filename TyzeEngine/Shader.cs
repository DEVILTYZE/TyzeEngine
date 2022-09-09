using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine;

internal sealed class Shader : IDisposable
{
    private bool _disposed;
    private readonly SortedList<string, int> _uniformLocations;
    private readonly string _name;

    internal string Error { get; private set; }
    internal int Handle { get; }
    internal bool IsEnabled { get; private set; }

    internal Shader(string vertexPath, string fragmentPath)
    {
        Error = string.Empty;

        if (!File.Exists(vertexPath))
            throw new FileNotFoundException("Vertex shader not found.", vertexPath);

        if (!File.Exists(fragmentPath))
            throw new FileNotFoundException("Fragment shader not found.", fragmentPath);

        _name = Path.GetFileNameWithoutExtension(vertexPath);
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
        _uniformLocations = new SortedList<string, int>();

        for (var i = 0; i < countOfUniforms; ++i)
        {
            var key = GL.GetActiveUniform(Handle, i, out _, out _);
            var location = GL.GetUniformLocation(Handle, key);
            _uniformLocations.Add(key, location);
        }
    }

    ~Shader() => ReleaseUnmanagedResources();
    
    internal void Enable()
    {
        IsEnabled = true;
        GL.UseProgram(Handle);
    }

    internal void Disable()
    {
        IsEnabled = false;
        GL.UseProgram(0);
    }

    internal int GetAttributeLocation(string parameterName) => GL.GetAttribLocation(Handle, parameterName);
    
    internal void SetMatrix3(string name, Matrix3 matrix)
    {
        if (!IsEnabled)
            Enable();
        
        GL.UniformMatrix3(_uniformLocations[name], true, ref matrix);
    }

    internal void SetMatrix4(string name, Matrix4 matrix)
    {
        if (!IsEnabled)
            Enable();
        
        GL.UniformMatrix4(_uniformLocations[name], true, ref matrix);
    }
    
    internal void SetVector3(string name, Vector3 vector)
    {
        if (!IsEnabled)
            Enable();
        
        GL.Uniform3(_uniformLocations[name], vector);
    }
    
    internal void SetVector4(string name, Vector4 vector)
    {
        if (!IsEnabled)
            Enable();
        
        GL.Uniform4(_uniformLocations[name], vector);
    }

    internal void SetLight(string name, ILight light)
    {
        if (!IsEnabled)
            Enable();
        
        GL.Uniform1(_uniformLocations[$"{name}.ambient"], light.Ambient);
        GL.Uniform1(_uniformLocations[$"{name}.specular"], light.Specular);
        GL.Uniform1(_uniformLocations[$"{name}.shininess"], light.Shininess);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public override string ToString() => Handle + " | " + _name;

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