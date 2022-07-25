using System;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace TyzeEngine.Resources;

public sealed class Texture : Resource
{
    private bool _disposed;
    
    public int Handle { get; private set; }
    public bool IsEnabled { get; private set; }
    public TextureUnit Unit { get; set; }

    public Texture(string path) : base(path)
    {
        Unit = TextureUnit.Texture0;
        Handle = -1;
        IsEnabled = false;
    }

    ~Texture() => Dispose(false);
    
    public override void Load()
    {
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

        var pixels = new byte[ConstHelper.ImageConst * image.Width * image.Height];
        image.CopyPixelDataTo(pixels);
        
        Handle = GL.GenTexture();
        Enable();
        
        // WRAPPING
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        
        // FILTERING
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        
        // GENERATE
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, 
            image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
    }

    public override void Enable()
    {
        if (Handle == ConstHelper.ErrorCode)
            Load();
        
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        GL.ActiveTexture(Unit);
        IsEnabled = true;
    }

    public override void Disable()
    {
        if (Handle == ConstHelper.ErrorCode)
            return;
        
        GL.BindTexture(TextureTarget.Texture2D, 0);
        IsEnabled = false;
    }

    protected override void Dispose(bool disposing)
    {
        if (_disposed) 
            return;
        
        Disable();
        GL.DeleteTexture(Handle);
        _disposed = true;
    }
}