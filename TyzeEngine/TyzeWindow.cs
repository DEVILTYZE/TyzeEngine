using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;

namespace TyzeEngine;

public class TyzeWindow : GameWindow
{
    private int _vertexBufferObject, _vertexArrayObject, _elementBufferObject;
    private readonly IReadOnlyList<IScene> _scenes;
    
    private Shader _shaderColor, _shaderTexture;
    private readonly float[] _vertices = { 
        .5f,  .5f,  .0f,
        .5f,  -.5f, .0f,
        -.5f, -.5f, .0f,
        -.5f, .5f,  .0f
    };
    
    private readonly uint[] _indices = {
        0, 1, 3,
        1, 2, 3
    };
    
    public TyzeWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, 
        IReadOnlyList<IScene> scenes) 
        : base(gameWindowSettings, nativeWindowSettings)
    {
        _scenes = scenes;
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(.2f, .3f, .3f, 1.0f);
        
        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, 
            BufferUsageHint.StaticDraw);
        
        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 
            0);
        GL.EnableVertexAttribArray(0);
        
        _elementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, 
            BufferUsageHint.StaticDraw);

        _shaderColor = new Shader(ConstHelper.ShaderVertColorPath, 
            ConstHelper.ShaderFragColorPath);
        _shaderColor.Use();
        _shaderTexture = new Shader(ConstHelper.ShaderVertTexturePath, 
            ConstHelper.ShaderFragTexturePath);
        _shaderTexture.Use();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        _shaderColor.Use();
        _shaderTexture.Use();
        
        GL.BindVertexArray(_vertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();

        // something...
    }

    protected override void OnResize(ResizeEventArgs args)
    {
        base.OnResize(args);
        GL.Viewport(0, 0, Size.X, Size.Y);
    }

    protected override void OnUnload()
    {
        _shaderColor.Dispose();
        _shaderTexture.Dispose();
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
        GL.UseProgram(0);
        
        GL.DeleteBuffer(_vertexBufferObject);
        GL.DeleteVertexArray(_vertexArrayObject);
        GL.DeleteProgram(_shaderColor.Handle);
        GL.DeleteProgram(_shaderTexture.Handle);
        
        base.OnUnload();
    }

    private void LoadObject(IGameObject obj)
    {
        
    }
}