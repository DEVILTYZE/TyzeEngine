using System;
using OpenTK.Mathematics;

namespace TyzeEngine.Interfaces;

public interface ICamera : IDisposable
{
    Vector3 Position { get; set; }
    Matrix4 ViewMatrix { get; }
}