using System;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public sealed class Camera : ICamera
{
    private static readonly Lazy<Camera> InstanceHolder = new(() => new Camera());
    public static ICamera Instance => InstanceHolder.Value;

    private bool _isInverted, _isFirstFrame = true;
    private Vector2 _lastPos;
    private Vector3 _direction, _up;
    private float _pitch, _yaw, _fov, _invertedControlCoef = 1;
    private Vector3 Right => Vector3.Cross(_direction, _up);

    public Vector3 Position { get; set; }
    public float AspectRatio { get; set; }
    public Matrix4 View => Matrix4.LookAt(Position, Position + _direction, _up);
    public Matrix4 Projection => Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, .01f, 100);
    
    public float Pitch
    {
        get => MathHelper.RadiansToDegrees(_pitch);
        set
        {
            var angle = MathHelper.Clamp(value, -89, 89);
            _pitch = MathHelper.DegreesToRadians(angle);
            Update();
        }
    }

    public float Yaw
    {
        get => MathHelper.RadiansToDegrees(_yaw);
        set
        {
            _yaw = MathHelper.DegreesToRadians(value % 360);
            Update();
        }
    }
    
    public float Fov
    {
        get => MathHelper.RadiansToDegrees(_fov);
        set
        {
            var angle = MathHelper.Clamp(value, 1, 90);
            _fov = MathHelper.DegreesToRadians(angle);
        }
    }

    public bool IsInverted
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

    private Camera() => ToDefault();

    public void Move(float speed, float time)
    {
        var speedAtTime = speed * time;
        
        if (Input.IsDown(KeyCode.W) || Input.IsDown(KeyCode.Up))
            Position += _direction * speedAtTime;
        else if (Input.IsDown(KeyCode.S) || Input.IsDown(KeyCode.Down))
            Position -= _direction * speedAtTime;
        if (Input.IsDown(KeyCode.D) || Input.IsDown(KeyCode.Right))
            Position += Right * speedAtTime;
        else if (Input.IsDown(KeyCode.A) || Input.IsDown(KeyCode.Left))
            Position -= Right * speedAtTime;
        if (Input.IsDown(KeyCode.Space) || Input.IsDown(KeyCode.NumPadNum0))
            Position += _up * speedAtTime;
        else if (Input.IsDown(KeyCode.LCtrl) || Input.IsDown(KeyCode.RCtrl))
            Position -= _up * speedAtTime;
    }

    public void Rotate(float sensitivity, float time)
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
    
    public void FocusToWorldCenter()
    {
        // VECTORS
        _direction = Vector3.NormalizeFast(Vector3.Zero - Position);
        var right = Vector3.NormalizeFast(Vector3.Cross(_direction, -Vector3.UnitY));
        _up = Vector3.NormalizeFast(Vector3.Cross(_direction, right));
        
        // ANGLES
        _pitch = MathF.Asin(_direction.Y);
        var angle = Vector3.CalculateAngle(Vector3.UnitZ, -_direction);

        if (_direction.X < 0)
            angle *= -1;
        
        _yaw = angle - MathHelper.PiOver2;
    }

    public void ToDefault()
    {
        Position = new Vector3(0, 0, 10);
        _isFirstFrame = true;
        _lastPos = Vector2.Zero;
        FocusToWorldCenter();
        Fov = Constants.FowDefault;
    }

    public override string ToString() => $"Camera.\r\nPos: {Position}\r\nDir: {_direction}\r\nUp: {_up}";

    private void Update()
    {
        var pitchCos = MathF.Cos(_pitch);
        var x = pitchCos * MathF.Cos(_yaw);
        var y = MathF.Sin(_pitch);
        var z = pitchCos * MathF.Sin(_yaw);
        
        _direction = Vector3.NormalizeFast(new Vector3(x, y, z));
        var right = Vector3.NormalizeFast(Vector3.Cross(_direction, -Vector3.UnitY));
        _up = Vector3.NormalizeFast(Vector3.Cross(_direction, right));
    }
}