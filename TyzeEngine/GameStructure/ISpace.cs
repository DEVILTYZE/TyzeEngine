using System;
using System.Collections.Generic;
using TyzeEngine.Interfaces;

namespace TyzeEngine.GameStructure;

public interface ISpace : IDisposable, IGameResource
{
    internal string SceneOrSpaceName { get; set; }
    
    List<ISpace> NeighbourSpaces { get; set; }
    List<IGameObject> GameObjects { get; set; }
    bool Loaded { get; set; }
}