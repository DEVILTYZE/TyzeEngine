using System;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TyzeEngine.Resources;

public sealed class Texture : Resource
{
    public bool IsEnabled { get; private set; }
    public TextureUnit Unit { get; set; }

    public Texture(string path) : base(path)
    {
        Unit = TextureUnit.Texture0;
        IsEnabled = false;
    }

    ~Texture() => Dispose(false);
    
    public override void Load()
    {
        Handle = GL.GenTexture();
        Enable();
        
        Image<Rgba32> image;
        
        try
        {
            image = Image.Load<Rgba32>(Path);
        }
        catch (Exception)
        {
            LoadError = true;
            return;
        }
        
        image.Mutate(img => img.Flip(FlipMode.Vertical));

        var pixels = new byte[Constants.ImageConst * image.Width * image.Height];
        image.CopyPixelDataTo(pixels);

        // GENERATE
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 
            0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        
        // FILTERING
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        
        // WRAPPING
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }

    public override void Enable()
    {
        GL.ActiveTexture(Unit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        IsEnabled = true;
    }

    public override void Disable()
    {
        if (Handle == Constants.ErrorCode)
            return;
        
        GL.BindTexture(TextureTarget.Texture2D, 0);
        IsEnabled = false;
    }

    protected override void Dispose(bool disposing)
    {
        if (Disposed) 
            return;
        
        Disable();
        Disposed = true;
    }
}