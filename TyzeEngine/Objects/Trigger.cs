using System;
using System.Diagnostics.CodeAnalysis;
using TyzeEngine.Interfaces;

namespace TyzeEngine.Objects;

public sealed class Trigger : ITrigger
{
    private event TriggerHandler Triggered;

    public UId Id { get; set; } = new();
    public bool IsTriggered { get; set; }
    public bool SaveStatus { get; }

    public Trigger(bool notSave = true) => SaveStatus = !notSave;

    public void AddScript(IScript script)
    {
        Triggered += script.Prepare;
        Triggered += script.Execute;
    }

    public void RemoveScript(IScript script)
    {
        Triggered -= script.Execute;
        Triggered -= script.Prepare;
    }

    public void OnTriggered()
    {
        Triggered?.Invoke();
        IsTriggered = true;
    }
    
    void IGameResource.Remove()
    {
    }

    /// <summary>
    /// Ищет триггер по имени среди всех добавленных в игру триггеров.
    /// </summary>
    /// <param name="name">Имя триггера.</param>
    /// <returns>Объект триггера, приведённый к типу ITrigger.</returns>
    /// <exception cref="ArgumentNullException">Имя равно null.</exception>
    /// <exception cref="Exception">Триггер не найден.</exception>
    public static ITrigger Find([NotNull] string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name), "Name was null.");
        
        var isFound = Game.Triggers.TryGetValue(name, out var value);

        return isFound ? value : throw new Exception("Trigger not found.");
    }
}