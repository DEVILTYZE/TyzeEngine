using OpenTK.Mathematics;

namespace TyzeEngine.Interfaces;

public interface ICamera
{
    Vector3 Position { get; set; }
    bool IsInverted { get; set; }
    float AspectRatio { get; set; }
    Matrix4 View { get; }
    Matrix4 Projection { get; }
    float Pitch { get; set; }
    float Yaw { get; set; }
    float Fov { get; set; }

    void Move(float speed, float time);
    void Rotate(float sensitivity, float time);
    void FocusToWorldCenter();
    void ToDefault();
}