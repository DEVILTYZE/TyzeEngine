using System;
using OpenTK.Mathematics;

namespace TyzeEngine.Objects;

public static class Camera
{
    private static bool _isInverted, _isFirstFrame = true;
    private static Vector2 _lastPos;
    private static Vector3 _up;
    private static float _pitch, _yaw, _fov, _invertedControlCoef = 1, _aspectRatio;
    private static Vector3 Right => Vector3.Cross(Direction, _up);

    public static Vector3 Position { get; set; }
    public static Vector3 Direction { get; private set; }
    public static Matrix4 View => Matrix4.LookAt(Position, Position + Direction, _up);
    public static Matrix4 Projection => Matrix4.CreatePerspectiveFieldOfView(_fov, _aspectRatio, .01f, 100);
    
    public static float Pitch
    {
        get => MathHelper.RadiansToDegrees(_pitch);
        set
        {
            var angle = MathHelper.Clamp(value, -89, 89);
            _pitch = MathHelper.DegreesToRadians(angle);
            Update();
        }
    }

    public static float Yaw
    {
        get => MathHelper.RadiansToDegrees(_yaw);
        set
        {
            _yaw = MathHelper.DegreesToRadians(value % 360);
            Update();
        }
    }
    
    public static float Fov
    {
        get => MathHelper.RadiansToDegrees(_fov);
        set
        {
            var angle = MathHelper.Clamp(value, 1, 90);
            _fov = MathHelper.DegreesToRadians(angle);
        }
    }

    public static bool IsInverted
    {
        get => _isInverted;
        set
        {
            if (value)
                _invertedControlCoef = -1;
            else
                _invertedControlCoef = 1;

            _isInverted = value;
        }
    }

    static Camera() => ToDefault();

    public static void Move(float speed, float time)
    {
        var speedAtTime = speed * time;
        
        if (Input.IsDown(KeyCode.W) || Input.IsDown(KeyCode.Up))
            Position += Direction * speedAtTime;
        else if (Input.IsDown(KeyCode.S) || Input.IsDown(KeyCode.Down))
            Position -= Direction * speedAtTime;
        if (Input.IsDown(KeyCode.D) || Input.IsDown(KeyCode.Right))
            Position += Right * speedAtTime;
        else if (Input.IsDown(KeyCode.A) || Input.IsDown(KeyCode.Left))
            Position -= Right * speedAtTime;
        if (Input.IsDown(KeyCode.Space) || Input.IsDown(KeyCode.NumPadNum0))
            Position += _up * speedAtTime;
        else if (Input.IsDown(KeyCode.LCtrl) || Input.IsDown(KeyCode.RCtrl))
            Position -= _up * speedAtTime;
    }

    public static void Rotate(float sensitivity, float time)
    {
        if (_isFirstFrame)
        {
            _lastPos = Input.MousePosition;
            _isFirstFrame = false;
            
            return;
        }
        
        var sensitivityAtTime = sensitivity * time;
        var deltaX = (Input.MousePosition.X - _lastPos.X) * sensitivityAtTime;
        var deltaY = (Input.MousePosition.Y - _lastPos.Y) * sensitivityAtTime * _invertedControlCoef;
        _lastPos = Input.MousePosition;
        
        Yaw += deltaX;
        Pitch += deltaY;
    }
    
    public static void FocusToWorldCenter()
    {
        // VECTORS
        Direction = Vector3.NormalizeFast(Vector3.Zero - Position);
        var right = Vector3.NormalizeFast(Vector3.Cross(Direction, -Vector3.UnitY));
        _up = Vector3.NormalizeFast(Vector3.Cross(Direction, right));
        
        // ANGLES
        _pitch = MathF.Asin(Direction.Y);
        var angle = Vector3.CalculateAngle(Vector3.UnitZ, -Direction);

        if (Direction.X < 0)
            angle *= -1;
        
        _yaw = angle - MathHelper.PiOver2;
    }

    public static void ToDefault()
    {
        Position = new Vector3(0, 0, 10);
        _isFirstFrame = true;
        _lastPos = Vector2.Zero;
        FocusToWorldCenter();
        Fov = Constants.FowDefault;
    }

    internal static void SetAspectRatio(float aspectRatio) => _aspectRatio = aspectRatio;

    private static void Update()
    {
        var pitchCos = MathF.Cos(_pitch);
        var x = pitchCos * MathF.Cos(_yaw);
        var y = MathF.Sin(_pitch);
        var z = pitchCos * MathF.Sin(_yaw);
        
        Direction = Vector3.NormalizeFast(new Vector3(x, y, z));
        var right = Vector3.NormalizeFast(Vector3.Cross(Direction, -Vector3.UnitY));
        _up = Vector3.NormalizeFast(Vector3.Cross(Direction, right));
    }
}