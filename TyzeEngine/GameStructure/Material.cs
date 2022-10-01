using System;
using System.Diagnostics.CodeAnalysis;
using TyzeEngine.Interfaces;

namespace TyzeEngine.GameStructure;

public abstract class Material : IMaterial
{
    public UId Id { get; set; } = new();
    public float Density { get; protected init; }
    public float Restitution { get; protected init; }
    public float StaticFriction { get; protected init; }
    public float DynamicFriction { get; protected init; }

    void IGameResource.Remove()
    {
    }
    
    /// <summary>
    /// Ищет материал по имени среди всех добавленных в игру материалов.
    /// </summary>
    /// <param name="name">Имя материала.</param>
    /// <returns>Объект материала, приведённый к типу IMaterial.</returns>
    /// <exception cref="ArgumentNullException">Имя равно null.</exception>
    /// <exception cref="Exception">Материал не найден.</exception>
    public static IMaterial Find([NotNull] string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name), "Name was null.");
        
        var isFound = Game.Materials.TryGetValue(name, out var value);

        return isFound ? value : throw new Exception("Material not found.");
    }
}