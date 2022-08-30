using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenTK.Windowing.Desktop;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;
using TyzeEngine.Physics;
using TyzeEngine.Resources;

namespace TyzeEngine;

public sealed class Game : IDisposable
{
    // Singleton field and property.
    private static readonly Lazy<Game> InstanceHolder = new(() => new Game());
    public static Game Instance => InstanceHolder.Value;

    #region PropertiesAndFields

    private Dictionary<UId, GameAssetType> _uIds = new();
    private Dictionary<string, GameAssetType> _names = new();
    private SortedList<string, IScene> _scenes = new();
    private SortedList<string, IPlace> _places = new();
    private SortedList<string, IGameObject> _objects = new();
    private SortedList<string, IScript> _scripts = new();
    private SortedList<string, ITrigger> _triggers = new();
    private SortedList<string, IResource> _resources = new();
    private SortedList<string, IModel> _models = new();
    private TyzeWindow _window;
    private bool _isRunning, _disposed;
    private List<IScript> _frameScripts = new();
    private readonly IReadOnlyDictionary<GameAssetType, IDictionary> _lists;

    internal IReadOnlyDictionary<string, IScene> Scenes => _scenes;
    internal IReadOnlyDictionary<string, IPlace> Places => _places;
    internal IReadOnlyDictionary<string, IGameObject> GameObjects => _objects;
    internal IReadOnlyDictionary<string, IScript> Scripts => _scripts;
    internal IReadOnlyDictionary<string, ITrigger> Triggers => _triggers;
    internal IReadOnlyDictionary<string, IResource> Resources => _resources;
    internal IReadOnlyDictionary<string, IModel> Models => _models;
    
    public RunMode RunMode { get; set; } = RunMode.Debug;
    public Saver Saver { get; set; } = new();
    
    public const string StandardSceneName = "StandardScene";
    public const string StandardPlaceName = "StandardPlace";

    #endregion

    private Game()
    {
        _lists = new SortedList<GameAssetType, IDictionary>
        {
            { GameAssetType.Scene, _scenes },
            { GameAssetType.Place, _places },
            { GameAssetType.GameObject, _objects },
            { GameAssetType.Script, _scripts },
            { GameAssetType.Trigger, _triggers },
            { GameAssetType.Resource, _resources },
            { GameAssetType.Model, _models }
        };
        var place = new Place();
        Add(StandardSceneName, new Scene(place));
        Add(StandardPlaceName, place, StandardSceneName);
    }

    ~Game() => Dispose(false);
    
    public void Run(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    {
        if (_isRunning)
            throw new Exception("Game is already running.");
        
        using (_window = new TyzeWindow(gameWindowSettings, nativeWindowSettings))
        {
            _window.IsDebugMode = RunMode == RunMode.Debug;
            _window.Scenes = _scenes.Select(pair => pair.Value).ToList();
            _window.Scripts = _frameScripts;
            _isRunning = true;
            _window.Run();
        }

        _isRunning = false;
    }

    #region SaveLoadMethods
    
    public void Save(string fileName)
    {
        if (_scenes.Count == 0 || _places.Count == 0 || _objects.Count == 0)
            return;

        var saveObject = new SaveObject(_window.CurrentSceneIndex, _places.Count, _objects.Count);

        foreach (var (_, place) in _places)
            saveObject.AddPlaceObjects(place);

        foreach (var (_, obj) in _objects.Where(localObj => localObj.Value.SaveStatus))
            saveObject.AddSaveObjectState(obj);

        var bytes = saveObject.Save();
        Saver.Save(bytes, fileName);
    }

    public bool Load(string fileName)
    {
        IMaterial BytesToMaterial(byte[] data)
        {
            var floats = Vector.BytesToFloats(data, 4).ToArray();
            
            return new Material(floats[0], floats[1], floats[2], floats[3]);
        }

        var withErrors = false;
        var bytes = Saver.Load(fileName);
        var saveObject = SaveObject.GetByBytes(bytes);
        _window.CurrentSceneIndex = saveObject.CurrentSceneIndex;
        
        foreach (var (_, obj) in _objects)
        {
            SaveObjectState state;

            try
            {
                state = saveObject.ObjectStates.First(localState => localState.Id == obj.Id);
            }
            catch (InvalidOperationException)
            {
                withErrors = true;
                continue;
            }

            obj.SetModel(_models.FirstOrDefault(model => model.Value.Id == state.ModelId).Value);
            obj.Texture = _resources.FirstOrDefault(resource => resource.Value.Id == state.ResourceId).Value;
            var type = Type.GetType(state.BodyTypeName);

            if (type is not null)
            {
                var body = (IBody)Activator.CreateInstance(type);

                if (body is not null)
                    obj.Body = body;
            }

            if (obj.Body is null)
            {
                withErrors = true;
                continue;
            }
            
            obj.Body.Position = Vector.ToVector3(state.Position);
            obj.Body.Scale = Vector.ToVector3(state.Scale);
            obj.Body.Rotation = Vector.ToQuaternion(state.Rotation);
            obj.Body.Color = Vector.ToVector4(state.Color);
            obj.Body.Visibility = (VisibilityType)state.VisibilityType;
            obj.Body.Visual = (BodyVisualType)state.Visual;
            obj.Body.CollisionLayer = state.CollisionLayer;
            obj.Body.SetMaterial(BytesToMaterial(state.Material));
            obj.Body.GravityForce = Vector.ToVector3(state.GravityForce);
            obj.Body.IsEnabled = state.IsEnabled;
        }
        
        foreach (var (_, place) in _places)
        {
            var index = -1;
            
            for (var i = 0; i < saveObject.PlaceIds.Length; ++i)
            {
                if (saveObject.PlaceIds[i] != place.Id) 
                    continue;
                
                index = i;
                break;
            }
            
            if (index == -1)
            {
                withErrors = true;
                continue;
            }

            foreach (var id in saveObject.PlaceObjects[index])
            {
                if (place.GameObjects.Any(obj => obj.Id == id))
                    continue;

                IGameObject obj;
                
                try
                {
                    obj = _objects.First(localObj => localObj.Value.Id == id).Value;
                }
                catch (InvalidOperationException)
                {
                    withErrors = true;
                    continue;
                }

                place.GameObjects.Add(obj);
            }
        }

        return !withErrors;
    }

    #endregion

    #region AddMethods
    
    public void Add([NotNull] string name, [NotNull] IScene scene)
    {
        TryAddName(name, GameAssetType.Scene);
        TryAddUId(scene, GameAssetType.Scene);
        _scenes.Add(name, scene);
    }

    public void Add([NotNull] string name, [NotNull] IPlace place, string sceneOrPlaceName)
    {
        if (_places.ContainsKey(sceneOrPlaceName))
            _places[sceneOrPlaceName].NeighbourPlaces.Add(place);
        else if (_scenes.ContainsKey(sceneOrPlaceName))
            _scenes[sceneOrPlaceName].CurrentPlace = place;
        else
            throw new ArgumentException("Scene or place with this name doesn't exists.", nameof(sceneOrPlaceName));
        
        TryAddName(name, GameAssetType.Place);
        TryAddUId(place, GameAssetType.Place);
        _places.Add(name, place);
    }
    
    public void Add([NotNull] string name, [NotNull] IGameObject obj, [NotNull] string placeName)
    {
        if (!_places.ContainsKey(placeName))
            throw new ArgumentException("Place with this name doesn't exists.", nameof(placeName));
        
        TryAddName(name, GameAssetType.GameObject);
        TryAddUId(obj, GameAssetType.GameObject);
        _places[placeName].GameObjects.Add(obj);
        _objects.Add(name, obj);
        LoadQueue.Add(obj);
    }
    
    public void Add([NotNull] string name, [NotNull] IScript script, bool isFrameDependent = true)
    {
        TryAddName(name, GameAssetType.Script);
        TryAddUId(script, GameAssetType.Script);
        
        if (isFrameDependent)
            _frameScripts.Add(script);
        
        _scripts.Add(name, script);
    }

    public void Add([NotNull] string name, [NotNull] ITrigger trigger)
    {
        TryAddName(name, GameAssetType.Trigger);
        TryAddUId(trigger, GameAssetType.Trigger);
        _triggers.Add(name, trigger);
    }

    public void Add([NotNull] string name, [NotNull] IResource resource)
    {
        if (_resources.Select(pair => pair.Value.Path).Any(path => string.CompareOrdinal(path, resource.Path) == 0))
            throw new ArgumentException("Resource with this path already exists.", nameof(resource));
        
        TryAddName(name, GameAssetType.Resource);
        TryAddUId(resource, GameAssetType.Resource);
        _resources.Add(name, resource);
    }

    public void Add([NotNull] string name, [NotNull] IModel model)
    {
        TryAddName(name, GameAssetType.Model);
        TryAddUId(model, GameAssetType.Model);
        _models.Add(name, model);
    }

    #endregion

    #region RemoveMethods

    public void Remove([NotNull] string name)
    {
        if (!_names.ContainsKey(name))
            return;
        
        var type = _names[name];
        _names.Remove(name);
        var obj = (IIdObject)_lists[type][name];
        
        switch (obj)
        {
            case null:
                return;
            case IScript script:
                _frameScripts.Remove(script);
                break;
        }

        _uIds.Remove(obj.Id);
        _lists[type].Remove(name);
    }

    public void Remove(UId id)
    {
        if (!_uIds.ContainsKey(id))
            return;
        
        var type = _uIds[id];
        _uIds.Remove(id);
        var name = string.Empty;
        var obj = new object();

        foreach (DictionaryEntry entry in _lists[type])
        {
            var idObj = (IIdObject)entry.Value;
            
            if (idObj is null)
                continue;
            
            if (idObj.Id != id)
                continue;

            name = (string)entry.Key;
            obj = entry.Value;
            break;
        }

        _names.Remove(name);
        
        if (obj is IScript script)
            _frameScripts.Remove(script);
        
        _lists[type].Remove(name);
    }

    #endregion
    
    public TriggerHandler GetTriggerLoadObjects() => _window.TriggerLoadObjects;

    public TriggerHandler GetTriggerNextScene() => _window.TriggerNextScene;
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void TryAddUId(IIdObject obj, GameAssetType type)
    {
        while (_uIds.ContainsKey(obj.Id))
            obj.Id = new UId();

        _uIds.Add(obj.Id, type);
    }
    
    private void TryAddName(string name, GameAssetType type)
    {
        if (_names.ContainsKey(name))
            throw new ArgumentException("Object with this name already exists.", nameof(name));

        _names.Add(name, type);
    }

    private void ReleaseUnmanagedResources() => _window?.Dispose();

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        
        if (disposing)
        {
            _uIds = null;
            _names = null;
            _scenes = null;
            _places = null;
            _objects = null;
            _triggers = null;
            _scripts = null;
            _frameScripts = null;
            _resources = null;
            _models = null;
            Saver = null;
        }
        
        ReleaseUnmanagedResources();
        _disposed = true;
    }
}