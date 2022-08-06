﻿using System.Collections.Generic;
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
    Uid ModelId { get; }
    List<Uid> ResourceIds { get; }
    IBody Body { get; set; }
    List<ITrigger> Triggers { get; }
    List<IScript> Scripts { get; }
    bool SaveStatus { get; }

    void EnableResources(Dictionary<Uid, IResource> resources);
    void DisableResources(Dictionary<Uid, IResource> resources);
}