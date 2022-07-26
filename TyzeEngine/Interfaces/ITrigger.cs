using System;

namespace TyzeEngine.Interfaces;

public class TriggeredEventArgs : EventArgs
{
    public object Data { get; }

    public TriggeredEventArgs(object data) => Data = data;
}

public interface ITrigger
{
    int Id { get; }
    bool IsTriggered { get; set; }
}