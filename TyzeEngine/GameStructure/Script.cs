using System;
using System.Diagnostics.CodeAnalysis;
using TyzeEngine.Interfaces;

namespace TyzeEngine.GameStructure;

public abstract class Script : IScript
{
    public UId Id { get; set; } = new();

    void IScript.InternalPrepare() => Prepare();

    void IScript.InternalExecute() => Execute();

    void IScript.InternalFixedExecute() => FixedExecute();

    protected abstract void Prepare();
    
    protected abstract void Execute();
    
    protected abstract void FixedExecute();

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