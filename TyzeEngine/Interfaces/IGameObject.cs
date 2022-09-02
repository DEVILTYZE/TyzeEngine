using System;
using OpenTK.Graphics.OpenGL4;

namespace TyzeEngine.Interfaces;

public interface IGameObject : IDisposable, IDeepCloneable<IGameObject>, IUIdObject
{
    internal ArrayObject ArrayObject { get; set; }
    internal BufferUsageHint DrawType { get; }
    
    IModel Model { get; }
    IBody Body { get; set; }
    ITransformation Transformation { get; }
    bool SaveStatus { get; set; }
    
    internal void SetModel(IModel model);
    internal void Load(Shader shader);
}