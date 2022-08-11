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
    IModel Model { get; }
    IBody Body { get; set; }
    List<Uid> ResourceIds { get; set; }
    Uid ResourceId { get; }
    List<ITrigger> Triggers { get; }
    List<IScript> Scripts { get; }
    bool SaveStatus { get; set; }

    void NextResource();
    void PreviousResource();
}