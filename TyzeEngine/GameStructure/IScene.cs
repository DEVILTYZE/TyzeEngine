using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

namespace TyzeEngine.GameStructure;

public interface IScene : IDisposable, IUIdObject
{
    internal Task LoadingPlacesTask { get; }
    
    bool LoadError { get; }
    ILighting Lighting { get; }
    IPlace CurrentPlace { get; set; }
    SortedList<UId, IResource> Resources { get; }
    SortedList<UId, IModel> Models { get; }
    TriggerHandler ReloadObjects { get; set; }
    TriggerHandler LoadSceneHandler { get; set; }

    void LoadPlace(TriggeredEventArgs args);
    void Run();
    void LoadResources();
}