using System;
using System.Collections.Generic;
using TyzeEngine.Objects;

namespace TyzeEngine.Interfaces;

public interface IGameObject : IDisposable, IDeepCloneable<IGameObject>, IGameResource
{
    internal string SpaceName { get; set; }
    
    IModel Model { get; set; }
    IBody Body { get; set; }
    ITransform Transform { get; }
    IVisual Visual { get; }
    bool SaveStatus { get; set; }

    internal void Draw(LightObject[] lights);
    internal void DrawLines();
}