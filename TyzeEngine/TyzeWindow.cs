using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;

namespace TyzeEngine;

public partial class TyzeWindow : GameWindow
{
    private readonly IReadOnlyList<IScene> _scenes;
    private BufferObject _vertexBufferObject, elementBufferObject;
    private ArrayObject _arrayObject;
    private Shader _shaderColor, _shaderTexture;
    
    private uint[] _indices; // remove
    private int _vertexArrayObject;

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

        _shaderColor = new Shader(
            Path.Combine(ConstHelper.AssetsDirectory, ConstHelper.ShadersDirectory, ConstHelper.ShaderVertColorPath), 
            Path.Combine(ConstHelper.AssetsDirectory, ConstHelper.ShadersDirectory,ConstHelper.ShaderFragColorPath));
        _shaderColor.Activate();
        _shaderTexture = new Shader(
            Path.Combine(ConstHelper.AssetsDirectory, ConstHelper.ShadersDirectory, ConstHelper.ShaderVertTexturePath), 
            Path.Combine(ConstHelper.AssetsDirectory, ConstHelper.ShadersDirectory, ConstHelper.ShaderFragTexturePath));
        _shaderTexture.Activate();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        _shaderColor.Activate();
        _shaderTexture.Activate();
        
        GL.BindVertexArray(_vertexArrayObject);
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);
        
        _shaderTexture.Deactivate();
        _shaderColor.Deactivate();

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
        
        
        base.OnUnload();
    }

    private void LoadObject(IGameObject obj)
    {
        
    }
}