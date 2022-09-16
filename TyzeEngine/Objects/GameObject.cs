using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class GameObject : IGameObject
{
    private bool _disposed;

    string IGameObject.SpaceName { get; set; }
    
    public UId Id { get; set; } = new();
    public IModel Model { get; set; }
    public IBody Body { get; set; }
    public ITransform Transform { get; private set; }
    public IVisual Visual { get; private set; }
    public bool SaveStatus { get; set; }

    protected GameObject()
    {
        Transform = new Transform();
        Visual = new Visual();
    }

    ~GameObject() => Dispose(false);

    public override string ToString()
        => $"object: {Id}\r\n" +
           string.Join(' ', Transform.Position) + "\r\n" +
           string.Join(' ', Transform.Scale) + "\r\n" +
           string.Join(' ', Transform.Rotation) + "\r\n" +
           string.Join(' ', Visual.Color) + "\r\n" +
           string.Join(' ', Visual.Visibility);
    
    public IGameObject Clone()
    {
        var obj = DeepClone();
        obj.Body = Body?.Clone();
        obj.Transform = Transform.Clone();
        obj.Model = Model;
        obj.Visual = Visual.Clone();
        obj.SaveStatus = SaveStatus;

        return obj;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Ищет игровой объект по имени среди всех добавленных в игру игровых объектов.
    /// </summary>
    /// <param name="name">Имя игрового объекта.</param>
    /// <returns>Игровой объект, приведённый к типу IGameObject.</returns>
    /// <exception cref="ArgumentNullException">Имя равно null.</exception>
    /// <exception cref="Exception">Игровой объект не найден.</exception>
    public static IGameObject Find([NotNull] string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name), "Name was null.");
        
        var isFound = Game.GameObjects.TryGetValue(name, out var value);

        return isFound ? value : throw new Exception("GameObject not found.");
    }

    public static IEnumerable<IGameObject> FindOrDefault(Predicate<IGameObject> predicate)
    {
        foreach (var (_, obj) in Game.GameObjects.Where(pair => predicate.Invoke(pair.Value)))
            yield return obj;
    }

    void IGameObject.Draw(IEnumerable<IGameObject> lights)
    {
        var shader = Game.Shaders[BodyVisualType.Object];
        var lighting = lights.First();
        var modelMatrix = GetModelMatrix();
        var viewMatrix = Camera.View;
        
        shader.Enable();
        Model?.ArrayObject.Enable();
        Visual.Texture?.Enable();
        shader.SetMatrix4("model", modelMatrix);
        shader.SetMatrix4("view", viewMatrix);
        shader.SetMatrix4("projection", Camera.Projection);
        shader.SetVector4("lightColor", Vector.ToVector4(lighting.Visual.Color));

        if (Visual.Type == BodyVisualType.Object)
        {
            shader.SetMatrix3("normalMatrix", new Matrix3(Matrix4.Transpose((viewMatrix * modelMatrix).Inverted())));
            shader.SetVector3("lightPosition", lighting.Transform.Position);
            shader.SetLight("light", Visual.Light);
            shader.SetVector4("inColor", Vector.ToVector4(Visual.Color));
        }
        
        Model?.ArrayObject.Draw(PrimitiveType.Triangles, 0, Model.Indices.Count, DrawElementsType.UnsignedInt);
        Visual.Texture?.Disable();
        shader.Disable();
    }

    void IGameObject.DrawLines()
    {
        var shader = Game.Shaders[BodyVisualType.Line];
        shader.Enable();
        Model.ArrayObject.Enable();
        shader.SetMatrix4("model", GetModelMatrix());
        shader.SetMatrix4("view", Camera.View);
        shader.SetMatrix4("projection", Camera.Projection);
        shader.SetVector4("inColor", new Vector4(1, 0, 0, 1));
        Model.ArrayObject.Draw(PrimitiveType.LineLoop, 0, Model.Indices.Count, DrawElementsType.UnsignedInt);
        shader.Disable();
    }

    void IGameResource.Remove()
    {
        var space = Game.Spaces[((IGameObject)this).SpaceName];
        space.GameObjects.Remove(this);
    }

    protected abstract GameObject DeepClone();

    private Matrix4 GetModelMatrix()
    {
        if (Body is null)
            return Transform.ModelMatrix;

        var time = FrameTimeState.RenderTime;
        // SCALE
        var scale = Matrix4.Identity * Transform.ScaleMatrix;
            
        // ROTATION
        var angularAcceleration = Body.Torque * Body.InverseInertia;
        Body.AngularVelocity += angularAcceleration * time;
        Transform.Rotation += Body.AngularVelocity * time;
        var rotation = scale * Transform.RotationMatrix;

        // TRANSLATION
        var acceleration = Body.Force * Body.InverseMass + Body.GravityForce;
        Body.Velocity += acceleration * time;
        Transform.Position += Body.Velocity * time;
            
        return rotation * Transform.TranslationMatrix;
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Body = null;
            Transform = null;
        }

        _disposed = true;
    }
}
