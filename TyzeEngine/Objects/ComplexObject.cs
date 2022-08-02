using System.Collections.Generic;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine.Objects;

public abstract class ComplexObject : GameObject, IComplexObject
{
    public List<IGameObject> GameObjects { get; }

    protected ComplexObject(Uid modelId, IReadOnlyList<Uid> resourceIds, IBody physics, List<IGameObject> objects) 
        : base(modelId, resourceIds, physics)
    {
        GameObjects = objects;
    }
}