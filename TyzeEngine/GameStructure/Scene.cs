using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using TyzeEngine.Interfaces;

namespace TyzeEngine.GameStructure;

public sealed class Scene : IScene
{
    private bool _loadError, _disposed;
    private Task _loadingSpacesTask;

    Task IScene.LoadingSpacesTask => _loadingSpacesTask;

    public UId Id { get; set; } = new();
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
    public ISpace CurrentSpace { get; set; }
    public SortedList<UId, IModel> Models { get; } = new();
    public TriggerHandler LoadSceneHandler { get; set; }
    
    public Scene() => CurrentSpace = new Space();

    ~Scene() => ReleaseUnmanagedResources();

    public void LoadSpace(TriggeredEventArgs args)
    {
        if (_loadingSpacesTask is not null && _loadingSpacesTask.Status == TaskStatus.Running)
            _loadingSpacesTask.Wait();
        
        _loadingSpacesTask = Task.Run(() => LoadSpace(args.ObjectId));
    }

    void IScene.Run()
    {
        ((IScene)this).LoadResources();
        LoadSpace(CurrentSpace.Id);
        
        // Other settings...
    }

    void IScene.LoadResources()
    {
        while (LoadQueue.HasNewModels)
        {
            var model = LoadQueue.TakeModel();
            
            if (!model.Loaded)
                model.LoadFromFile();
            
            model.Load();
            Models.Add(model.Id, model);
        }
    }

    IReadOnlyList<IGameObject> IScene.GetCurrentGameObjects()
    {
        var objects = new List<IGameObject>(CurrentSpace.GameObjects);
        CurrentSpace.NeighbourSpaces.ForEach(space => objects.AddRange(space.GameObjects));

        return objects;
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    void IGameResource.Remove()
    {
    }

    /// <summary>
    /// Ищет сцену по имени среди всех добавленных в игру сцен.
    /// </summary>
    /// <param name="name">Имя сцены.</param>
    /// <returns>Объект сцены, приведённый к типу IScene.</returns>
    /// <exception cref="ArgumentNullException">Имя равно null.</exception>
    /// <exception cref="Exception">Сцена не найдена.</exception>
    public static IScene Find([NotNull] string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name), "Name was null.");
        
        var isFound = Game.Scenes.TryGetValue(name, out var value);

        return isFound ? value : throw new Exception("Scene not found.");
    }

    private void LoadSpace(object obj)
    {
        var id = (UId)obj;
        ISpace space = null;

        if (CurrentSpace.Id != id)
        {
            foreach (var neighbourSpace in CurrentSpace.NeighbourSpaces)
            {
                if (neighbourSpace.Id != id)
                    neighbourSpace.Loaded = false;
                else
                {
                    space = neighbourSpace;
                    break;
                }
            }

            if (space is null)
            {
                LoadError = true;
                return;
            }
        }
        else
            space = CurrentSpace;

        space.Loaded = true;
        space.NeighbourSpaces.ForEach(localSpace => localSpace.Loaded = true);
    }

    private void ReleaseUnmanagedResources()
    {
        if (_disposed)
            return;
        
        Models.ToList().ForEach(pair => pair.Value?.Dispose());
        _disposed = true;
    }
}