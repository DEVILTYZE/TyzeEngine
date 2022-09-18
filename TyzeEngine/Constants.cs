using System;
using OpenTK.Mathematics;

namespace TyzeEngine;

internal static class Constants
{
    // DIRECTORIES
    internal const string AssetsDirectory = "Assets\\";
    internal const string ModelsDirectory = AssetsDirectory + "Models\\";
    internal const string SystemDirectory = AssetsDirectory + "System\\";
    internal const string DefaultModelsDirectory = ModelsDirectory + "Default\\";
    internal const string ShadersDirectory = "Shaders\\";
    internal const string SavesDirectory = SystemDirectory + "Saves\\";
    
    // FILES
    internal const string ShaderVertLinePath = ShadersDirectory + "ShaderLine.vert";
    internal const string ShaderFragLinePath = ShadersDirectory + "ShaderLine.frag";
    internal const string ShaderVertObjectPath = ShadersDirectory + "ShaderObject.vert";
    internal const string ShaderFragObjectPath = ShadersDirectory + "ShaderObject.frag";
    internal const string FileNotExists = SystemDirectory + "Error.png"; // Path
    internal const string DefaultModelName = "model";
    internal static readonly string DefaultSaveName = "save_" + DateTime.Now.ToString("ddMMyyyyHHmmss");
    internal const string SaveExtension = ".sav";
    
    // DEFAULT VALUES
    internal const int MaxLightCount = 32;
    internal const double FixedTimeLimit = .01;
    internal const float YawDefault = -45;
    internal const float FowDefault = 45;
    internal const float OneSecond = 1;
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
    internal static readonly Vector3 DefaultRotation = Vector3.Zero;
    internal static readonly Color4 DefaultColor = new(.5f, .5f, .5f, 1);
    internal static readonly Color4 NullColor = new(0, 0, 0, 0);
    internal static readonly Color4 DarkSpaceColor = new(.15f, .15f, .15f, 1);
    internal static readonly Color4 LightSpaceColor = new(.85f, .85f, .85f, 1);
    
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