using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Physics.Bodies;

public class PlaneBody : Body
{
    public Vector3 Plane { get; }
    public float Distance { get; }
    
    public PlaneBody(IMaterial material, Vector3 plane = default, float distance = 1) : base(material, 3)
    {
        Plane = plane;
        Distance = distance;
        var mass = distance * distance * material.Density;
        SetMassAndInertia(mass, mass);
    }

    public override IBody Clone(IBody obj = null) => 
        (PlaneBody)base.Clone(obj ?? new PlaneBody(Material, Plane, Distance));
}