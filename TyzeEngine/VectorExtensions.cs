using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;

namespace TyzeEngine;

internal static class Vector
{
    internal static Vector2 ToVector2(byte[] data)
    {
        var floats = ToFloats(data, Constants.Vector2Length).ToArray();
            
        return new Vector2(floats[0], floats[1]);
    }
    
    internal static Vector3 ToVector3(byte[] data)
    {
        var floats = ToFloats(data, Constants.Vector3Length).ToArray();
            
        return new Vector3(floats[0], floats[1], floats[2]);
    }

    internal static Vector4 ToVector4(byte[] data)
    {
        var floats = ToFloats(data, Constants.Vector4Length).ToArray();
            
        return new Vector4(floats[0], floats[1], floats[2], floats[3]);
    }

    internal static Vector4 ToVector4(Color4 color) => new(color.R, color.G, color.B, color.A);

    internal static Quaternion ToQuaternion(byte[] data)
    {
        var floats = ToFloats(data, Constants.Vector4Length).ToArray();
        
        return new Quaternion(floats[0], floats[1], floats[2], floats[3]);
    }
    
    internal static byte[] ToBytes(Vector2 vector)
    {
        var (x, y) = vector;

        return GetBytes(new[] { x, y }, Constants.Vector2Stride);
    }
    
    internal static byte[] ToBytes(Vector3 vector)
    {
        var (x, y, z) = vector;

        return GetBytes(new[] { x, y, z }, Constants.Vector3Stride);
    }
    
    internal static byte[] ToBytes(Vector4 vector)
    {
        var (x, y, z, w) = vector;

        return GetBytes(new[] { x, y, z, w }, Constants.Vector4Stride);
    }

    internal static byte[] ToBytes(Quaternion quaternion)
    {
        var (x, y, z) = quaternion.Xyz;
        var w = quaternion.W;
        
        return GetBytes(new[] { x, y, z, w }, Constants.Vector4Stride);
    }

    internal static byte[] GetBytes(IReadOnlyList<float> floats, int vectorLength)
    {
        var data = new byte[vectorLength];

        for (var i = 0; i < vectorLength; ++i)
        {
            var floatNumInBytes = BitConverter.GetBytes(floats[i]);

            for (int j = sizeof(float) * i, j2 = 0; j2 < sizeof(float); ++j, ++j2)
                data[j] = floatNumInBytes[j2];
        }

        return data;
    }

    internal static IEnumerable<float> ToFloats(Vector3 vector) => new[] { vector.X, vector.Y, vector.Z };

    internal static IEnumerable<float> ToFloats(Vector4 vector) => new[] { vector.X, vector.Y, vector.Z, vector.W };

    private static IEnumerable<float> ToFloats(byte[] data, int length)
    {
        const int stride = sizeof(float);

        for (var i = 0; i < length; ++i)
            yield return BitConverter.ToSingle(data, i * stride);
    }
}