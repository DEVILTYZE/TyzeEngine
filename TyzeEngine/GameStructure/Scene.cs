using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;
using TyzeEngine.Resources;

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
        private init => _loadError = value;
    }

    public ILighting Lighting { get; }
    public IPlace CurrentPlace { get; }
    public List<IResource> Resources { get; }
    public TriggerHandler ReloadObjects { get; set; }
    public TriggerHandler LoadSceneHandler { get; set; }
    
    public Scene(IPlace spawnPlace)
    {
        Resources = new List<IResource>();
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
        LoadResources();
        LoadPlace(CurrentPlace.Id);
        
        // Other settings...
    }

    public void LoadScene(int index) => LoadSceneHandler?.Invoke(new TriggeredEventArgs(index));

    public void LoadResources()
    {
        while (LoadQueue.HasNewResources)
        {
            var resource = LoadQueue.TakeLast();
            resource.Load();
            Resources.Add(resource);
        }
    }
    
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

        var resourceIds = new HashSet<Uid>(place.GetResourceIds());

        foreach (var neighbourPlace in place.NeighbourPlaces)
        {
            if (neighbourPlace.Loaded)
                continue;

            foreach (var resource in neighbourPlace.GetResourceIds())
                resourceIds.Add(resource);
        }
        
        LoadResources(resourceIds);
    }
    
    private void LoadResources(IEnumerable<Uid> ids)
    {
        var resources = Resources.Where(resource => ids.Any(id => id.Equals(resource.Id)));

        foreach (var resource in resources)
            resource.Load();
    }
}