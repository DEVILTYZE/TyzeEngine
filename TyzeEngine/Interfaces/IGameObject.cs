using System;
using OpenTK.Graphics.OpenGL4;
using TyzeEngine.Resources;

namespace TyzeEngine.Interfaces;

public interface IGameObject : IDisposable, IDeepCloneable<IGameObject>, IIdObject
{
    internal ArrayObject ArrayObject { get; set; }
    internal BufferUsageHint DrawType { get; }
    
    IModel Model { get; }
    IBody Body { get; set; }
    IResource Texture { get; set; }
    bool SaveStatus { get; set; }
    
    internal void SetModel(IModel model);
}