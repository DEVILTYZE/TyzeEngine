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
    IObjectPhysics Physics { get; set; }
    List<ITrigger> Triggers { get; }
    List<IScript> Scripts { get; }
    Vector3 Position { get; }
    Vector3 Size { get; }
    Vector3 Rotation { get; }
    Vector4 Color { get; }
    bool HasTexture { get; }
    VisibilityType Visibility { get; }

    void TranslateTo(float x, float y, float z);
    void ScaleTo(float x, float y, float z);
    void RotateTo(float x, float y, float z);
    void Translate(float x, float y, float z);
    void Scale(float x, float y, float z);
    void Rotate(float x, float y, float z);
    void ChangeColor(byte r, byte g, byte b, byte a);
    void RemoveColor();
    void ChangeTextureStatus(bool isEnabled, bool withColor = false);
    void ChangeVisibility(VisibilityType newType);
    void EnableResources(Dictionary<Uid, IResource> resources);
    void DisableResources(Dictionary<Uid, IResource> resources);
}