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
    
    private static readonly Action<IDictionary, string, object> AddMethod = (dictionary, name, obj) =>
    {
        if (dictionary.Contains(name))
            dictionary[name] = obj;
        else
            dictionary.Add(name, obj);
    };

    private static HashSet<UId> _uIds = new();
    private static HashSet<string> _names = new();
    private static SortedList<string, IScene> _scenes = new();
    private static SortedList<string, IPlace> _places = new();
    private static SortedList<string, IGameObject> _objects = new();
    private static SortedList<string, IScript> _scripts = new();
    private static SortedList<string, IScript> _frameScripts = new();
    private static SortedList<string, ITrigger> _triggers = new();
    private static SortedList<string, IResource> _resources = new();
    private static SortedList<string, IModel> _models = new();
    private static TyzeWindow _window;
    private static bool _isRunning, _disposed;

    internal static IReadOnlyDictionary<string, IScene> Scenes => _scenes;
    internal static IReadOnlyDictionary<string, IPlace> Places => _places;
    internal static IReadOnlyDictionary<string, IGameObject> GameObjects => _objects;
    internal static IReadOnlyDictionary<string, IScript> Scripts => _scripts.Concat(_frameScripts).ToDictionary(
        item => item.Key, item => item.Value);
    internal static IReadOnlyDictionary<string, ITrigger> Triggers => _triggers;
    internal static IReadOnlyDictionary<string, IResource> Resources => _resources;
    internal static IReadOnlyDictionary<string, IModel> Models => _models;
    public RunMode RunMode { get; set; } = RunMode.Debug;
    public Saver Saver { get; set; } = new();
    
    public const string StandardSceneName = "StandardScene";
    public const string StandardPlaceName = "StandardPlace";

    #endregion

    private Game()
    {
        var place = new Place();
        var scene = new Scene(place);
        Add(StandardSceneName, scene);
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
            _window.Scripts = _frameScripts.Select(pair => pair.Value).ToList();
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
            obj.Triggers.AddRange(_triggers.Where(trigger 
                => state.TriggersIds.Any(id => id == trigger.Value.Id)).Select(pair => pair.Value));
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
            obj.Body.Rotation = Vector.ToVector3(state.Rotation);
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
    
    public void Add(string name, [NotNull] IScene scene)
    {
        TryAddName(name);
        TryAddUid(scene);
        AddMethod.Invoke(_scenes, name, scene);
    }

    public void Add(string name, [NotNull] IPlace place, string sceneOrPlaceName)
    {
        if (_places.ContainsKey(sceneOrPlaceName))
            _places[sceneOrPlaceName].NeighbourPlaces.Add(place);
        else if (_scenes.ContainsKey(sceneOrPlaceName))
            _scenes[sceneOrPlaceName].CurrentPlace = place;
        else
            throw new ArgumentException("Scene or place with this name doesn't exists.", nameof(sceneOrPlaceName));
        
        TryAddName(name);
        TryAddUid(place);
        AddMethod.Invoke(_places, name, place);
    }
    
    public void Add(string name, [NotNull] IGameObject obj, string placeName)
    {
        if (!_places.ContainsKey(placeName))
            throw new ArgumentException("Place with this name doesn't exists.", nameof(placeName));
        
        TryAddName(name);
        TryAddUid(obj);
        _places[placeName].GameObjects.Add(obj);
        AddMethod.Invoke(_objects, name, obj);
    }
    
    public void Add(string name, [NotNull] IScript script, bool isFrameDependent = true)
    {
        TryAddName(name);
        TryAddUid(script);
        AddMethod.Invoke(isFrameDependent ? _frameScripts : _scripts, name, script);
    }

    public void Add(string name, [NotNull] ITrigger trigger, string objectName)
    {
        if (!_objects.ContainsKey(objectName))
            throw new ArgumentException("Object with this name doesn't exists.", nameof(objectName));
        
        TryAddName(name);
        TryAddUid(trigger);
        _objects[objectName].Triggers.Add(trigger);
        AddMethod.Invoke(_triggers, name, trigger);
    }

    public void Add(string name, [NotNull] IResource resource)
    {
        if (_resources.Select(pair => pair.Value.Path).Any(path => string.CompareOrdinal(path, resource.Path) == 0))
            throw new ArgumentException("Resource with this path already exists.", nameof(resource));
        
        TryAddName(name);
        TryAddUid(resource);
        AddMethod.Invoke(_resources, name, resource);
    }

    public void Add(string name, IModel model)
    {
        TryAddName(name);
        TryAddUid(model);
        AddMethod.Invoke(_models, name, model);
    }

    #endregion

    public TriggerHandler GetTriggerLoadObjects() => _window.TriggerLoadObjects;

    public TriggerHandler GetTriggerNextScene() => _window.TriggerNextScene;
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private static void TryAddUid(IIdObject obj)
    {
        while (_uIds.Contains(obj.Id))
            obj.Id = new UId();

        _uIds.Add(obj.Id);
    }
    
    private static void TryAddName(string name)
    {
        if (_names.Contains(name))
            throw new ArgumentException("Object with this name already exists.", nameof(name));

        _names.Add(name);
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
        }
        
        ReleaseUnmanagedResources();
        _disposed = true;
    }
}