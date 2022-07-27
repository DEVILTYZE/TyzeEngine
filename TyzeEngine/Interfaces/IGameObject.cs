using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using TyzeEngine.Resources;

namespace TyzeEngine.Interfaces;

public interface IGameObject
{
    internal ArrayObject ArrayObject { get; set; }
    internal BufferUsageHint DrawType { get; }
    
    int Id { get; }
    IModel Model { get; }
    IReadOnlyList<Uid> ResourceIds { get; }
    IPhysics Physics { get; }
    List<ITrigger> Triggers { get; }
    List<IScript> Scripts { get; }

    void EnableResources(List<IResource> resources);
    void DisableResources(List<IResource> resources);
}