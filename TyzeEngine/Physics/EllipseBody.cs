using System;
using TyzeEngine.Materials;
using TyzeEngine.Objects;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace TyzeEngine.Physics;

public class EllipseBody : Body
{
    public float Radius { get; private set; }
    
    public EllipseBody(IMaterial material, float radius = 1) : base(material)
    {
        Radius = radius;
        ComputeMass();
    }

    protected override Body DeepClone() => new EllipseBody(Material, Radius);

    protected sealed override void ComputeMass()
    {
        var r2 = Radius * Radius;
        SetMassAndInertia(MathF.PI * r2 * Material.Density, .5f * Mass * r2);
    }

    internal override void RecomputeBodyParameters(Vector3 newScale)
    {
        if (Radius == 0)
            return;
        
        var maxComponent = newScale.X > newScale.Y ? newScale.X : newScale.Y;
        maxComponent = maxComponent > newScale.Z ? maxComponent : newScale.Z; 
        Radius *= maxComponent;
        ComputeMass();
    }
}

