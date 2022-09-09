using System;
using System.Collections.Generic;

namespace TyzeEngine.Interfaces;

public interface IGameObject : IDisposable, IDeepCloneable<IGameObject>, IGameResource
{
    internal string SpaceName { get; set; }
    
    IModel Model { get; set; }
    IBody Body { get; set; }
    ITransform Transform { get; }
    IVisual Visual { get; }
    bool SaveStatus { get; set; }

    internal void Load();
    internal void Draw(IEnumerable<IGameObject> lights);
}