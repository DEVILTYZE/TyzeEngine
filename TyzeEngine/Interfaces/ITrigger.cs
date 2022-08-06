using System;

namespace TyzeEngine.Interfaces;

public interface ITrigger
{
    Uid Id { get; }
    bool IsTriggered { get; set; }
    bool SaveStatus { get; }

    void OnTriggered();
}