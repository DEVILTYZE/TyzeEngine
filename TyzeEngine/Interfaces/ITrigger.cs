using System;

namespace TyzeEngine.Interfaces;

public interface ITrigger
{
    int Id { get; }
    bool IsTriggered { get; set; }
}