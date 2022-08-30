using System;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public sealed class Camera : ICamera
{
    private static readonly Lazy<Camera> InstanceHolder = new(() => new Camera());
    public static ICamera Instance => InstanceHolder.Value;

    public Vector3 Position { get; set; } = new(0, 0, 10);
    public Matrix4 ViewMatrix => Matrix4.CreateTranslation(-Position);

    private Camera()
    {
        
    }

    public void Dispose()
    {
    }

}