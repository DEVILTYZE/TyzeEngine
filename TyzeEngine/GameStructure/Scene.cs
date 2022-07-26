using System;
using System.Threading;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;

namespace TyzeEngine.GameStructure;

public class Scene : IScene
{
    private bool _loadError;
    private Thread _loadingPlacesThread;

    Thread IScene.LoadingPlacesThread => _loadingPlacesThread;

    public bool LoadError
    {
        get
        {
            var value = _loadError;
            _loadError = false;

            return value;
        }
        private set => _loadError = value;
    }

    public ILighting Lighting { get; }
    public IPlace CurrentPlace { get; }
    public TriggerHandler ReloadObjects { get; set; }
    public TriggerHandler LoadSceneHandler { get; set; }
    
    public Scene(IPlace spawnPlace)
    {
        CurrentPlace = spawnPlace;
        LoadError = false;
    }
    
    public void LoadPlace(TriggeredEventArgs args)
    {
        if (_loadingPlacesThread is not null && _loadingPlacesThread.IsAlive)
            _loadingPlacesThread.Join();
        
        _loadingPlacesThread = new Thread(LoadPlace);
        _loadingPlacesThread.Start((int)args.Data);
    }

    public void Start()
    {
        LoadPlace(CurrentPlace.Id);
        
        // Other settings...
    }

    public void LoadScene(int index) => LoadSceneHandler?.Invoke(new TriggeredEventArgs(index));
    
    private void LoadPlace(object obj)
    {
        var id = (int)obj;
        IPlace place = null;

        if (CurrentPlace.Id != id)
        {
            foreach (var neighbourPlace in CurrentPlace.NeighbourPlaces)
            {
                if (neighbourPlace.Id != id)
                {
                    (neighbourPlace as IDisposable)?.Dispose();
                    neighbourPlace.Loaded = false;
                }
                else
                    place = neighbourPlace;
            }

            GC.Collect();
            
            if (place is null)
                return;
        }
        else
            place = CurrentPlace;
        
        foreach (var neighbourPlace in place.NeighbourPlaces)
        {
            if (neighbourPlace.Loaded)
                continue;

            LoadPlace(neighbourPlace);
        }
        
        LoadPlace(place);
    }

    private void LoadPlace(IPlace place)
    {
        foreach (var localObj in place.Objects)
        {
            localObj.Load();
                
            if (!localObj.LoadError)
                LoadError = true;
        }

        place.Loaded = true;
    }
}