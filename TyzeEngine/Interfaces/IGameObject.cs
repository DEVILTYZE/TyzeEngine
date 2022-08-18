using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using TyzeEngine.Resources;

namespace TyzeEngine.Interfaces;

public interface IGameObject : IDisposable
{
    internal ArrayObject ArrayObject { get; set; }
    internal BufferUsageHint DrawType { get; }
    
    Uid Id { get; }
    IModel Model { get; }
    IBody Body { get; set; }
    IResource Texture { get; set; }
    List<ITrigger> Triggers { get; }
    List<IScript> Scripts { get; }
    bool SaveStatus { get; set; }
}