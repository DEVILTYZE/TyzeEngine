using System;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TyzeEngine;

public class Texture : IDisposable
{
    private bool _disposed;
    
    public int Handle { get; }

    public Texture(string path)
    {
        Handle = GL.GenTexture();
        Use();

        var image = Image.Load<Rgba32>(path);
        image.Mutate(img => img.Flip(FlipMode.Vertical));

        var pixels = new byte[ConstHelper.ImageConst * image.Width * image.Height];
        image.CopyPixelDataTo(pixels);
        
        // WRAPPING
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        
        // FILTERING
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        
        // GENERATE
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, 
            image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
    }
    
    ~Texture() => GL.DeleteProgram(Handle);
    
    public void Use() => GL.BindTexture(TextureTarget.Texture2D, Handle);

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