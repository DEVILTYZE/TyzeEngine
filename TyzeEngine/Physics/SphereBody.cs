using System;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics;

public class SphereBody : Body
{
    public Vector3 Center { get; }
    public float Radius { get; }

    public SphereBody(IMaterial material, Vector3 center, float radius) : base(material, 3)
    {
        if (radius < 0)
            throw new ArgumentOutOfRangeException(nameof(radius), "Radius less then zero.");

        Center = center;
        Radius = radius;
    }

    public override IBody Clone(IBody obj = null) => 
        (SphereBody)base.Clone(obj ?? new SphereBody(Material, Center, Radius));
}