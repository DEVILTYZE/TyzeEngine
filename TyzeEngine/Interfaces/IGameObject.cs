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
    IReadOnlyList<IResource> Resources { get; }
    IPhysics Physics { get; }
    List<ITrigger> Triggers { get; }
    List<IScript> Scripts { get; }
    bool LoadError { get; }

    void Load();
    void EnableResources();
}