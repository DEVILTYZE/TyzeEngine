using System;
using OpenTK.Mathematics;

namespace TyzeEngine;

internal static class Constants
{
    // DIRECTORIES
    internal const string AssetsDirectory = "assets\\";
    internal const string ModelsDirectory = AssetsDirectory + "models\\";
    internal const string SystemDirectory = AssetsDirectory + "system\\";
    internal const string DefaultModelsDirectory = ModelsDirectory + "default\\";
    internal const string ShadersDirectory = AssetsDirectory + "shaders\\";
    internal const string SavesDirectory = SystemDirectory + "saves\\";
    
    // FILES
    internal const string ShaderVertTexturePath = ShadersDirectory + "shader.vert";
    internal const string ShaderFragTexturePath = ShadersDirectory + "shader.frag";
    internal const string FileNotExists = SystemDirectory + "error.png"; // Path
    internal const string DefaultModelName = "model";
    internal static readonly string DefaultSaveName = "save_" + DateTime.Now.ToString("ddMMyyyyHHmmss");
    internal const string SaveExtension = ".sav";
    
    // DEFAULT VALUES
    internal const int DefaultDelay = 25;
    internal const int ErrorCode = -1;
    internal const int Vector2Length = 2;
    internal const int Vector2Stride = Vector2Length * sizeof(float);
    internal const int Vector3Length = 3;
    internal const int Vector3Stride = Vector3Length * sizeof(float);
    internal const int Vector4Length = 4;
    internal const int Vector4Stride = Vector4Length * sizeof(float);
    internal const int VectorStride = Vector3Stride + Vector2Stride + Vector4Stride;
    internal static readonly Vector3 DefaultPosition = new(0, 0, 0);
    internal static readonly Vector3 DefaultSize = new(1, 1, 1);
    internal static readonly Vector3 DefaultSize2D = new(1, 1, 0);
    internal static readonly Quaternion DefaultRotation = new(0, 0, 0);
    internal static readonly Vector4 DefaultColor = new(.5f, .5f, .5f, 1);
    internal static readonly Vector4 NullColor = new(0, 0, 0, 0);
    
    // AUDIO
    internal const int Frequency441 = 44100;
    internal const int BitDepth8 = 8;
    internal const int BitDepth16 = BitDepth8 * 2;
    internal const float Duration = .8f;

    // RESOURCE
    internal static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".ico", ".tiff", ".svg" };
    internal static readonly string[] AudioExtensions = { ".mp3", ".wav", ".flac", ".aiff" };
    internal static readonly string[] VideoExtensions = { ".avi", ".mp4", ".mov", ".flv", ".wmv", ".mpeg" };
}