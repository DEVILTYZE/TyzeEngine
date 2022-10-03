using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TyzeEngine.Interfaces;

public interface IScene : IDisposable, IGameResource
{
    internal Task LoadingSpacesTask { get; }
    
    bool LoadError { get; }
    ISpace CurrentSpace { get; set; }
    SortedList<UID, IModel> Models { get; }
    TriggerHandler LoadSceneHandler { get; set; }

    void LoadSpace(TriggeredEventArgs args);
    internal void Run();
    internal void LoadResources();
    internal IReadOnlyList<IGameObject> GetCurrentGameObjects();
}