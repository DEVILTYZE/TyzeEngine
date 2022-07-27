using System.Collections.Generic;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class ComplexObject : GameObject, IComplexObject
{
    public List<IGameObject> Objects { get; }

    protected ComplexObject(int id, IReadOnlyList<Uid> resourceIds, IPhysics physics, List<IGameObject> objects) 
        : base(id, null, resourceIds, physics)
    {
        Objects = objects;
    }
}