using System;
using OpenTK.Mathematics;

namespace TyzeEngine;

public static class Constants
{
    // DIRECTORIES
    public const string AssetsDirectory = "assets\\";
    public const string ModelsDirectory = AssetsDirectory + "models\\";
    public const string SystemDirectory = AssetsDirectory + "system\\";
    public const string DefaultModelsDirectory = ModelsDirectory + "default\\";
    public const string ShadersDirectory = AssetsDirectory + "shaders\\";
    public const string SavesDirectory = SystemDirectory + "saves\\";
    
    // FILES
    public const string ShaderVertTexturePath = ShadersDirectory + "shader.vert";
    public const string ShaderFragTexturePath = ShadersDirectory + "shader.frag";
    public const string FileNotExists = SystemDirectory + "error.png"; // Path
    public const string DefaultModelName = "model";
    public const string PointModelName = "point";
    public const string TriangleModelName = "triangle";
    public const string RectangleModelName = "rectangle";
    public const string CircleModelName = "circle";
    public const string CubeModelName = "cube";
    public const string ModelExtension = ".mdl";
    public static readonly string DefaultSaveName = "save_" + DateTime.Now.ToString("ddMMyyyyHHmmss");
    public const string SaveExtension = ".sav";

    public const string TestName = "Test application";
    public const int DefaultDelay = 25;
    public const int ImageConst = 4;
    public const int ErrorCode = -1;
    public const int SizeOfTrigger = sizeof(int) + sizeof(bool);
    public const int VertexStride = VertexLength * sizeof(float);
    public const int VertexLength = 3;
    public const int ColorStride = ColorLength * sizeof(float);
    public const int ColorLength = 4;
    public const int TextureStride = TextureLength * sizeof(float);
    public const int TextureLength = 2;
    internal static readonly Vector3 DefaultPosition = new(0, 0, 0);
    internal static readonly Vector3 DefaultSize = new(1, 1, 1);
    internal static readonly Vector3 DefaultSize2D = new(1, 1, 0);
    internal static readonly Vector3 DefaultRotation = new(0, 0, 0);
    internal static readonly Vector4 DefaultColor = new(.5f, .5f, .5f, 1);
    internal static readonly Vector4 NullColor = new(0, 0, 0, 0);
    public const float GravityConstant = .00000000667f;
    
    // AUDIO
    public const int Frequency441 = 44100;
    public const int Frequency48 = 48000;
    public const int BitDepth8 = 8;
    public const int BitDepth16 = BitDepth8 * 2;
    public const int BitDepth24 = BitDepth8 * 3;
    public const int BitDepth32 = BitDepth8 * 4;
    public const float Duration = .75f;

    public static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".ico", ".tiff", ".svg" };
    public static readonly string[] AudioExtensions = { ".mp3", ".wav", ".flac", ".aiff" };
    public static readonly string[] VideoExtensions = { ".avi", ".mp4", ".mov", ".flv", ".wmv", ".mpeg" };

    public const string VerticesName = "vertices";
    public const string IndicesName = "indices";
    public const string PositionName = "position";
    public const string ColorName = "color";
}