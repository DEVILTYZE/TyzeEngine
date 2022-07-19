using System;
using System.Threading;

namespace TyzeEngine.GameStructure;

public class Scene : IScene
{
    private bool _loadError;
    private Thread _loadingPlacesThread;

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

    public Scene(IPlace spawnPlace)
    {
        CurrentPlace = spawnPlace;
        LoadError = false;
    }
    
    public void LoadPlace(int id)
    {
        if (_loadingPlacesThread is not null && _loadingPlacesThread.IsAlive)
            _loadingPlacesThread.Join();
        
        _loadingPlacesThread = new Thread(LoadPlace);
        _loadingPlacesThread.Start(id);
    }

    public void Start()
    {
        LoadPlace(CurrentPlace.Id);
        
        // Other settings...
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
        
        foreach (var neighbourPlace in place.NeighbourPlaces)
        {
            if (neighbourPlace.Loaded)
                continue;

            foreach (var localObj in neighbourPlace.Objects)
            {
                localObj.Load();
                
                if (!localObj.LoadError)
                    LoadError = true;
            }

            neighbourPlace.Loaded = true;
        }
    }
}