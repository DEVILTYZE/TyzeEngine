using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenTK.Mathematics;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine.GameStructure;

public class GameObject : IGameObject
{
    string IGameObject.SpaceName { get; set; }

    public UId Id { get; set; } = new();
    public IModel Model { get; set; }
    public IBody Body { get; set; }
    public ITransform Transform { get; private set; }
    // Нулевой цвет даёт возможность изначально отображать текстуру, если цвет не задан.
    public Color4 Color { get; set; } = Constants.NullColor;
    public Visibility Visibility { get; set; } = Visibility.Visible;
    public BodyVisualType VisualType { get; protected internal set; } = BodyVisualType.Object;
    public bool SaveStatus { get; set; }
    public bool IsTrigger { get; set; }
    public CollisionHandler OnCollision { get; set; }

    public GameObject() => Transform = new Transform();

    public CollisionPoints TestCollision(ITransform transform, IGameObject obj, ITransform bodyTransform)
    {
        var points = Body.TestCollision(transform, obj.Body, bodyTransform);

        return points.IsCollides ? Model.TestCollision(transform, obj.Model, bodyTransform) : CollisionPoints.NonCollides;
    }

    public override string ToString() => 
        $"{GetType().Name} | {Model.GetType().Name}: {Id.ShortValue}\r\n" + 
        $"{Transform}\r\nClr: {Color}\r\nVis: {Visibility}" +
        (Body is null ? "\r\nBody is null" : $"\r\n + {Body.GetType().Name}");

    public virtual IGameObject Clone(IGameObject obj = null)
    {
        var currentObj = (GameObject)obj ?? new GameObject();
        currentObj.Body = Body?.Clone();
        currentObj.Transform = Transform.Clone();
        currentObj.Model = Model;
        currentObj.Color = Color;
        currentObj.Visibility = Visibility;
        currentObj.VisualType = VisualType;
        currentObj.SaveStatus = SaveStatus;

        return currentObj;
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

    public static IEnumerable<IGameObject> GetAll() => Game.GameObjects.Select(pair => pair.Value);

    void IGameObject.Draw(List<ILightObject> lights)
    {
        if (Model is null || Visibility != Visibility.Visible)
            return;

        var count = new[] { 0, 0 };
        var modelMatrix = Transform.ModelMatrix;
        var viewMatrix = Camera.View;
        var shader = Game.Shaders[BodyVisualType.Object];
        shader.Enable();
        shader.SetMatrix4("model", modelMatrix);
        shader.SetMatrix4("view", viewMatrix);
        shader.SetMatrix4("projection", Camera.Projection);
        shader.SetVector4("inColor", Vector.ToVector4(Color));
        shader.SetVector3("viewPos", Camera.Position);

        if (VisualType == BodyVisualType.Object)
        {
            lights.ForEach(light => shader.SetLight(light.GetName(count), light));
            shader.SetMatrix4("normalMatrix", modelMatrix.Inverted());
            shader.SetInt("dirCount", count[0]);
            shader.SetInt("pointCount", count[1]);
        }
        
        Model.Draw(shader);
        shader.Disable();
    }

    void IGameObject.DrawLines()
    {
        if (Model is null)
            return;
        
        var shader = Game.Shaders[BodyVisualType.Line];
        shader.Enable();
        shader.SetMatrix4("model", Transform.ModelMatrix);
        shader.SetMatrix4("view", Camera.View);
        shader.SetMatrix4("projection", Camera.Projection);
        shader.SetVector4("inColor", new Vector4(1, 0, 0, 1));
        Model.DrawLines();
        shader.Disable();
    }

    void IGameResource.Remove() => Game.Spaces[((IGameObject)this).SpaceName].GameObjects.Remove(this);
}
