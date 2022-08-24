using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TyzeEngine.Interfaces;
using TyzeEngine.Resources;

namespace TyzeEngine.GameStructure;

public sealed class Scene : IScene
{
    private bool _loadError, _disposed;
    private Task _loadingPlacesTask;

    Task IScene.LoadingPlacesTask => _loadingPlacesTask;

    public UId Id { get; set; } = new();
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
    public IPlace CurrentPlace { get; set; }
    public SortedList<UId, IResource> Resources { get; } = new();
    public SortedList<UId, IModel> Models { get; } = new();
    public TriggerHandler ReloadObjects { get; set; }
    public TriggerHandler LoadSceneHandler { get; set; }
    
    public Scene(IPlace spawnPlace)
    {
        CurrentPlace = spawnPlace;
        LoadError = false;
    }

    ~Scene() => ReleaseUnmanagedResources();

    public void LoadPlace(TriggeredEventArgs args)
    {
        if (_loadingPlacesTask is not null && _loadingPlacesTask.Status == TaskStatus.Running)
            _loadingPlacesTask.Wait();
        
        _loadingPlacesTask = Task.Run(() => LoadPlace((int)args.Data));
    }

    public void Run()
    {
        LoadResources();
        LoadPlace(CurrentPlace.Id);
        
        // Other settings...
    }

    public void LoadScene(UId id) => LoadSceneHandler?.Invoke(new TriggeredEventArgs(id));

    public void LoadResources()
    {
        while (LoadQueue.HasNewResources)
        {
            var resource = LoadQueue.TakeResource();
            resource.Load();
            Resources.Add(resource.Id, resource);
        }

        while (LoadQueue.HasNewModels)
        {
            var model = LoadQueue.TakeModel();
            model.Load();
            Models.Add(model.Id, model);
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    public static IScene Find(string name)
    {
        var isFound = Game.Scenes.TryGetValue(name, out var value);

        return isFound ? value : null;
    }

    private void LoadPlace(object obj)
    {
        var id = (UId)obj;
        IPlace place = null;

        if (CurrentPlace.Id != id)
        {
            foreach (var neighbourPlace in CurrentPlace.NeighbourPlaces)
            {
                if (neighbourPlace.Id != id)
                {
                    neighbourPlace.Dispose();
                    neighbourPlace.Loaded = false;
                }
                else
                {
                    place = neighbourPlace;
                    break;
                }
            }

            if (place is null)
                return;
        }
        else
            place = CurrentPlace;

        var resourceIds = new HashSet<UId>(place.GetResourceIds());

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
    
    private void LoadResources(IEnumerable<UId> ids)
    {
        foreach (var id in ids)
            Resources[id].Load();
    }

    private void ReleaseUnmanagedResources()
    {
        if (_disposed)
            return;
        
        var places = new[] { CurrentPlace }.Concat(CurrentPlace.NeighbourPlaces);

        foreach (var place in places)
            place?.Dispose();
        
        foreach (var (_, resource) in Resources)
            resource?.Dispose();

        _disposed = true;
    }
}