using System;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TyzeEngine.Resources;

public sealed class Texture : Resource
{
    private bool _disposed;
    
    public int Handle { get; }
    public bool IsEnabled { get; private set; }


    public Texture(string path) : base(path)
    {
        IsEnabled = false;
        Image<Rgba32> image;
        
        try
        {
            image = Image.Load<Rgba32>(path);
        }
        catch (Exception)
        {
            LoadError = true;
            return;
        }
        
        image.Mutate(img => img.Flip(FlipMode.Vertical));

        var pixels = new byte[ConstHelper.ImageConst * image.Width * image.Height];
        image.CopyPixelDataTo(pixels);
        
        Handle = GL.GenTexture();
        Load();
        
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
    
    public override void Load()
    {
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        IsEnabled = true;
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed) 
            return;
        
        IsEnabled = false;
        GL.DeleteProgram(Handle);
        _disposed = true;
    }

}