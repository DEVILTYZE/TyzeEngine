using System.Collections.Generic;

namespace TyzeEngine.Interfaces;

public interface ISpace : IGameResource
{
    internal string SceneOrSpaceName { get; set; }
    
    List<ISpace> NeighbourSpaces { get; set; }
    List<IGameObject> GameObjects { get; set; }
    bool Loaded { get; set; }
}