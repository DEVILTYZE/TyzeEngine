namespace TyzeEngine;

public static class ConstHelper
{
    // DIRECTORIES
    public const string AssetsDirectory = "assets\\";
    public const string ModelsDirectory = "models\\";
    public const string SystemDirectory = "system\\";
    public const string DefaultModelsDirectory = "default\\";
    public const string ShadersDirectory = "shaders\\";
    
    // FILES
    public const string ShaderVertColorPath = "assets\\shaders\\shader_color.vert";
    public const string ShaderFragColorPath = "assets\\shaders\\shader_color.frag";
    public const string ShaderVertTexturePath = "assets\\shaders\\shader_texture.vert";
    public const string ShaderFragTexturePath = "assets\\shaders\\shader_texture.frag";
    public const string FileNotExists = "error.png"; // Path
    public const string DefaultModelName = "model";
    public const string ModelExtension = ".mdl";

    public const int DefaultDelay = 25;
    public const int ImageConst = 4;
    public const int ErrorCodeBuffer = -1;

    public static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".ico", ".tiff", ".svg" };
    public static readonly string[] AudioExtensions = { ".mp3", ".wav", ".flac", ".aiff" };
    public static readonly string[] VideoExtensions = { ".avi", ".mp4", ".mov", ".flv", ".wmv", ".mpeg" };
    public static readonly byte[] Separator = { byte.MaxValue, byte.MinValue, byte.MaxValue, byte.MinValue };
}