using System.Threading;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine.GameStructure;

public interface IScene
{
    internal Thread LoadingPlacesThread { get; }
    
    bool LoadError { get; }
    ILighting Lighting { get; }
    IPlace CurrentPlace { get; }
    TriggerHandler ReloadObjects { get; set; }
    TriggerHandler LoadSceneHandler { get; set; }

    void LoadPlace(EventTriggeredArgs args);
    void Start();
    void LoadScene(int index);
}