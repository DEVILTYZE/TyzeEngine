using System;
using System.Collections.Generic;
using System.Threading;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

namespace TyzeEngine.GameStructure;

public sealed class Scene : IScene
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
    public Dictionary<Uid, IResource> Resources { get; }
    public Dictionary<Uid, IModel> Models { get; }
    public TriggerHandler ReloadObjects { get; set; }
    public TriggerHandler LoadSceneHandler { get; set; }
    
    public Scene(IPlace spawnPlace)
    {
        Resources = new Dictionary<Uid, IResource>();
        Models = new Dictionary<Uid, IModel>();
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

    public void Run()
    {
        LoadResources();
        LoadPlace(CurrentPlace.Id);
        
        // Other settings...
    }

    public void LoadScene(Uid id) => LoadSceneHandler?.Invoke(new TriggeredEventArgs(id));

    public void LoadResources()
    {
        while (LoadQueue.HasNewResources)
        {
            var resource = LoadQueue.TakeLastResource();
            resource.Load();
            Resources.Add(resource.Id, resource);
        }

        while (LoadQueue.HasNewModels)
        {
            var model = LoadQueue.TakeLastModel();
            model.Load();
            Models.Add(model.Id, model);
        }
    }

    public void EnableResource(Uid id)
    {
        if (Resources.ContainsKey(id))
            Resources[id].Enable();
    }

    public void DisableResource(Uid id)
    {
        if (Resources.ContainsKey(id))
            Resources[id].Disable();
    }

    private void LoadPlace(object obj)
    {
        var id = (Uid)obj;
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
        place.Loaded = true;

        foreach (var neighbourPlace in place.NeighbourPlaces)
            neighbourPlace.Loaded = true;
    }
    
    private void LoadResources(IEnumerable<Uid> ids)
    {
        foreach (var id in ids)
            Resources[id].Load();
    }
}