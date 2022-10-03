using System;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics.Bodies;

public class CircleBody : Body
{
    public float Radius { get; }
    
    public CircleBody(IMaterial material, float radius = 1) : base(material, 2)
    {
        if (radius < 0)
            throw new ArgumentException("Radius less than zero.");

        Radius = radius;
        var area = MathF.PI * Radius * Radius;
        SetMassAndInertia(area, area);
    }

    public override IBody Clone(IBody obj = null) => (CircleBody)base.Clone(obj ?? new CircleBody(Material, Radius));
}
