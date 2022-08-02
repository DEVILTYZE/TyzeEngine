using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TyzeEngine.Physics;
using TyzeEngine.Resources;

namespace TyzeEngine.Interfaces;

public interface IGameObject
{
    internal ArrayObject ArrayObject { get; set; }
    internal BufferUsageHint DrawType { get; }
    
    Uid Id { get; }
    Uid ModelId { get; }
    IReadOnlyList<Uid> ResourceIds { get; }
    IBody Body { get; set; }
    List<ITrigger> Triggers { get; }
    List<IScript> Scripts { get; }
    Vector3 Position { get; }
    Vector3 Size { get; }
    Vector3 Rotation { get; }
    Vector4 Color { get; }
    bool HasTexture { get; }
    VisibilityType Visibility { get; set; }

    void SetPosition(float x, float y, float z);
    void SetScale(float x, float y, float z);
    void SetRotation(float x, float y, float z);
    void Translate(float x, float y, float z);
    void Scale(float x, float y, float z);
    void Rotate(float x, float y, float z);
    void SetColor(byte r, byte g, byte b, byte a);
    void RemoveColor();
    void SetTextureStatus(bool isEnabled, bool withColor = false);
    void EnableResources(Dictionary<Uid, IResource> resources);
    void DisableResources(Dictionary<Uid, IResource> resources);
}