using System.Collections.Generic;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class ComplexObject : GameObject, IComplexObject
{
    public List<IGameObject> GameObjects { get; }

    protected ComplexObject(IModel model, List<IGameObject> objects) 
        : base(model)
    {
        GameObjects = objects;
    }
}