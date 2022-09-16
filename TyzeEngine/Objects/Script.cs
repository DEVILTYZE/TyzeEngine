using System;
using System.Diagnostics.CodeAnalysis;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public abstract class Script : IScript
{
    public UId Id { get; set; } = new();

    public abstract void Prepare();
    
    public abstract void Execute();
    
    public abstract void FixedExecute();

    void IGameResource.Remove()
    {
        if (Game.FrameScripts.Contains(this))
            Game.FrameScripts.Remove(this);
    }

    /// <summary>
    /// Ищет скрипт по имени среди всех добавленных в игру скриптов.
    /// </summary>
    /// <param name="name">Имя скрипта.</param>
    /// <returns>Объект скрипта, приведённый к типу IScript.</returns>
    /// <exception cref="ArgumentNullException">Имя равно null.</exception>
    /// <exception cref="Exception">Скрипт не найден.</exception>
    public static IScript Find([NotNull] string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name), "Name was null.");
        
        var isFound = Game.Scripts.TryGetValue(name, out var value);

        return isFound ? value : throw new Exception("Script not found.");
    }
}