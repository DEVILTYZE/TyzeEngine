using System.Collections.Generic;
using OpenTK.Mathematics;
using TyzeEngine.Physics;

namespace TyzeEngine.Interfaces;

public interface IGameObject : IDeepCloneable<IGameObject>, IGameResource
{
    internal string SpaceName { get; set; }
    
    IModel Model { get; set; }
    IBody Body { get; set; }
    ITransform Transform { get; }
    Color4 Color { get; set; }
    Visibility Visibility { get; set; }
    BodyVisualType VisualType { get; }
    bool SaveStatus { get; set; }
    bool IsTrigger { get; set; }
    CollisionState CollisionState { get; }
    CollisionHandler OnCollision { get; set; }
    
    CollisionPoints TestCollision(ITransform transform, IGameObject obj, ITransform bodyTransform);
    internal void Draw(List<ILightObject> lights);
    internal void DrawLines();
}