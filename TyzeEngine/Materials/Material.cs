using System;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Materials;

public abstract class Material : IMaterial
{
    public UId Id { get; set; } = new();
    public float Density { get; protected init; }
    public float Restitution { get; protected init; }
    public float StaticFriction { get; protected init; }
    public float DynamicFriction { get; protected init; }

    void IGameResource.Remove()
    {
    }

    public static IMaterial Find(string name)
    {
        var isFound = Game.Materials.TryGetValue(name, out var value);

        if (isFound)
            return value;

        throw new Exception("Material not found.");
    }
}