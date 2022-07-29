using System.Collections.Generic;
using System.Threading;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;
using TyzeEngine.Resources;

namespace TyzeEngine.GameStructure;

public interface IScene
{
    internal Thread LoadingPlacesThread { get; }
    
    bool LoadError { get; }
    ILighting Lighting { get; }
    IPlace CurrentPlace { get; }
    Dictionary<Uid, IResource> Resources { get; }
    Dictionary<Uid, IModel> Models { get; }
    TriggerHandler ReloadObjects { get; set; }
    TriggerHandler LoadSceneHandler { get; set; }

    void LoadPlace(TriggeredEventArgs args);
    void Start();
    void LoadScene(int index);
    void LoadResources();
}