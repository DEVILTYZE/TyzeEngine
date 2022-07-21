using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class ComplexObject : GameObject, IComplexObject
{
    public List<IGameObject> Objects { get; }
    public override bool LoadError => Objects.Any(obj => obj.LoadError);

    protected ComplexObject(int id, IEnumerable<string> resourcePath, IPhysics physics, List<IGameObject> objects) 
        : base(id, null, resourcePath, physics)
    {
        Objects = objects;
    }

    public override void Load()
    {
        foreach (var obj in Objects)
            ThreadPool.QueueUserWorkItem(_ => obj.Load());
    }
}