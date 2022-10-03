using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

namespace TyzeEngine;

internal sealed class Shader : IDisposable
{
    private bool _disposed;
    private readonly string _name;
    private readonly SortedList<string, int> _uniformLocations;

    internal string Error { get; private set; }
    internal int Handle { get; private set; }
    internal bool IsEnabled { get; private set; }

    internal Shader(string vertexPath, string fragmentPath)
    {
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
            Error = GL.GetProgramInfoLog(Handle);
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
            throw new Exception("Shader is not enabled.");
        
        GL.UniformMatrix3(_uniformLocations[name], false, ref matrix);
    }

    internal void SetMatrix4(string name, Matrix4 matrix)
    {
        if (!IsEnabled)
            throw new Exception("Shader is not enabled.");
        
        GL.UniformMatrix4(_uniformLocations[name], false, ref matrix);
    }
    
    internal void SetVector3(string name, Vector3 vector)
    {
        if (!IsEnabled)
            throw new Exception("Shader is not enabled.");
        
        GL.Uniform3(_uniformLocations[name], vector);
    }
    
    internal void SetVector4(string name, Vector4 vector)
    {
        if (!IsEnabled)
            throw new Exception("Shader is not enabled.");
        
        GL.Uniform4(_uniformLocations[name], vector);
    }

    internal void SetInt(string name, int value)
    {
        if (!IsEnabled)
            throw new Exception("Shader is not enabled.");
        
        GL.Uniform1(_uniformLocations[name], value);
    }

    internal void SetLight(string name, ILightObject lightObject)
    {
        if (!IsEnabled)
            throw new Exception("Shader is not enabled.");
        
        GL.Uniform3(_uniformLocations[$"{name}.ambient"], lightObject.Ambient);
        GL.Uniform3(_uniformLocations[$"{name}.diffuse"], lightObject.Diffuse);
        GL.Uniform3(_uniformLocations[$"{name}.specular"], lightObject.Specular);

        if (name.StartsWith("dir"))
        {
            GL.Uniform3(_uniformLocations[$"{name}.direction"], lightObject.Direction);
        }
        else if (name.StartsWith("point"))
        {
            GL.Uniform3(_uniformLocations[$"{name}.position"], lightObject.Transform.Position);
            GL.Uniform1(_uniformLocations[$"{name}.constant"], lightObject.Constant);
            GL.Uniform1(_uniformLocations[$"{name}.linear"], lightObject.Linear);
            GL.Uniform1(_uniformLocations[$"{name}.quadratic"], lightObject.Quadratic);
        }
        else if (name.StartsWith("spot"))
        {
            GL.Uniform3(_uniformLocations[$"{name}.position"], lightObject.Transform.Position);
            GL.Uniform3(_uniformLocations[$"{name}.direction"], lightObject.Direction);
            GL.Uniform1(_uniformLocations[$"{name}.cutOff"], lightObject.CutOff);
            GL.Uniform1(_uniformLocations[$"{name}.outerCutOff"], lightObject.OuterCutOff);
        }
    }

    internal void SetTexture(string name, int number, Texture texture)
    {
        if (texture is null)
            throw new ArgumentException("Texture is null.", nameof(texture));

        var location = texture.IsDiffuse
            ? _uniformLocations[$"{name}.diffuse{number}"]
            : _uniformLocations[$"{name}.specular{number}"];
        GL.Uniform1(location, texture.UnitNumber);
        GL.Uniform1(_uniformLocations[$"{name}.shininess"], texture.Shininess);
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