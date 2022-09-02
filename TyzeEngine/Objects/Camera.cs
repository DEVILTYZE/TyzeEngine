using System;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public sealed class Camera : ICamera
{
    private static readonly Lazy<Camera> InstanceHolder = new(() => new Camera());
    public static ICamera Instance => InstanceHolder.Value;

    private Vector3 _direction, _up;

    public static Vector3 ForwardVector => new(0, 0, -1);
    public static Vector3 UpVector => new(0, 1, 0);
    public static Vector3 RightVector => new(1, 0, 0);
    
    public Vector3 Position { get; set; }
    public Vector3 Direction { get => _direction; set => _direction = Vector3.NormalizeFast(value); }
    public Vector3 Up { get => _up; set => _up = Vector3.NormalizeFast(value); }
    public Matrix4 ViewMatrix => Matrix4.LookAt(Position, Position + Direction, Up);

    private Camera() => ToDefault();

    public void Rotate(Vector3 eulerAngles)
    {
        var matrix = Matrix3.CreateFromQuaternion(Quaternion.FromEulerAngles(eulerAngles));
        Direction *= matrix;
        Up *= matrix;
    }

    public void ControlDefault(float speed, float straightAngle, float sideAngle)
    {
        if (Input.IsDown(KeyCode.W))
            Position += ForwardVector * speed;
        else if (Input.IsDown(KeyCode.S))
            Position -= ForwardVector * speed;
        if (Input.IsDown(KeyCode.D))
            Position += RightVector * speed;
        else if (Input.IsDown(KeyCode.A))
            Position -= RightVector * speed;
        if (Input.IsDown(KeyCode.Space))
            Position += UpVector * speed;
        else if (Input.IsDown(KeyCode.LCtrl))
            Position -= UpVector * speed;

        if (Input.IsMouseUpNow)
        {
            Direction += UpVector * straightAngle;
            Up -= ForwardVector * straightAngle;
        }
        else if (Input.IsMouseDownNow)
        {
            Direction -= UpVector * straightAngle;
            Up += ForwardVector * straightAngle;
        }
        if (Input.IsMouseRightNow)
            Direction += RightVector * sideAngle;
        else if (Input.IsMouseLeftNow)
            Direction -= RightVector * sideAngle;
    }

    public void ToDefault()
    {
        Position = new Vector3(0, 0, 10);
        Direction = Vector3.NormalizeFast(Vector3.Zero - Position);
        var right = Vector3.NormalizeFast(Vector3.Cross(new Vector3(0, 1, 0), Direction));
        Up = Vector3.Cross(Direction, right);
    }

    public override string ToString() => $"Camera.\r\nPos: {Position}\r\nDir: {Direction}\r\nUp: {Up}";
}