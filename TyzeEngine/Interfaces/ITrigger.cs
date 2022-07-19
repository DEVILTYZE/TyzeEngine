namespace TyzeEngine.Interfaces;

public interface ITrigger
{
    delegate void TriggeredEvent(object obj = null);
    delegate void TriggeredLoadEvent(int id);
    delegate void TriggeredWorkEvent();
}