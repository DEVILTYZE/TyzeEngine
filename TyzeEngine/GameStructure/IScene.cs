using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

namespace TyzeEngine.GameStructure;

public interface IScene : IDisposable, IGameResource
{
    internal Task LoadingSpacesTask { get; }
    
    bool LoadError { get; }
    ISpace CurrentSpace { get; set; }
    SortedList<UId, IModel> Models { get; }
    TriggerHandler LoadSceneHandler { get; set; }

    void LoadSpace(TriggeredEventArgs args);
    internal void Run();
    internal void LoadResources();
    internal IReadOnlyList<IGameObject> GetCurrentGameObjects();
}