using System;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics.Bodies;

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
        var volume = MathF.PI / 3 * Radius * Radius * Radius * material.Density;
        SetMassAndInertia(volume, volume);
    }

    public override IBody Clone(IBody obj = null) => 
        (SphereBody)base.Clone(obj ?? new SphereBody(Material, Center, Radius));
}