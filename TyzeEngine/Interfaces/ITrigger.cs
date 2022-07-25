using System;

namespace TyzeEngine.Interfaces;

public class EventTriggeredArgs : EventArgs
{
    public int PlaceId { get; }
    public bool IsLoadTrigger { get; }
    public object Data { get; }

    public EventTriggeredArgs(object data)
    {
        Data = data;
        IsLoadTrigger = false;
    }
    
    public EventTriggeredArgs(int placeId)
    {
        if (placeId == -1)
            IsLoadTrigger = false;
        else
        {
            PlaceId = placeId;
            IsLoadTrigger = true;
        }
    }
}

public interface ITrigger
{
    int Id { get; }
    bool IsTriggered { get; set; }
}