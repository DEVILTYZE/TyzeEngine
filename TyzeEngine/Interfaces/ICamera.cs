using OpenTK.Mathematics;

namespace TyzeEngine.Interfaces;

public interface ICamera
{
    Vector3 Position { get; set; }
    Vector3 Direction { get; set; }
    Vector3 Up { get; set; }
    Matrix4 ViewMatrix { get; }

    void Rotate(Vector3 eulerAngles);
    void ControlDefault(float speed, float straightAngle, float sideAngle);
    void ToDefault();
}