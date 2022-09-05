using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Windowing.Desktop;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;
using TyzeEngine.Objects;
using TyzeEngine.Physics;
using TyzeEngine.Resources;

namespace TyzeEngine;

public static class Game
{
    #region PropertiesAndFields

    private static readonly Dictionary<UId, GameAssetType> UIds = new();
    private static readonly Dictionary<string, GameAssetType> Names = new();
    private static readonly SortedList<string, IScene> PrivateScenes = new();
    private static readonly SortedList<string, IPlace> PrivatePlaces = new();
    private static readonly SortedList<string, IGameObject> Objects = new();
    private static readonly SortedList<string, IScript> PrivateScripts = new();
    private static readonly SortedList<string, ITrigger> PrivateTriggers = new();
    private static readonly SortedList<string, IResource> PrivateResources = new();
    private static readonly SortedList<string, IModel> PrivateModels = new();
    private static bool _isRunning;
    private static int _currentSceneIndex;
    private static readonly List<IScript> FrameScripts = new();
    private static readonly IReadOnlyDictionary<GameAssetType, IDictionary> Lists;
    private static Stopwatch _fixedTime;
    private static readonly Task FixedExecute = new(() =>
    {
        while (_isRunning)
        {
            if (_fixedTime.Elapsed.TotalSeconds <= FixedTime) 
                continue;
            
            foreach (var script in FrameScripts)
                script.FixedExecute();
            
            _fixedTime.Restart();
        }
    });

    internal static IReadOnlyDictionary<string, IScene> Scenes => PrivateScenes;
    internal static IReadOnlyDictionary<string, IPlace> Places => PrivatePlaces;
    internal static IReadOnlyDictionary<string, IGameObject> GameObjects => Objects;
    internal static IReadOnlyDictionary<string, IScript> Scripts => PrivateScripts;
    internal static IReadOnlyDictionary<string, ITrigger> Triggers => PrivateTriggers;
    internal static IReadOnlyDictionary<string, IResource> Resources => PrivateResources;
    internal static IReadOnlyDictionary<string, IModel> Models => PrivateModels;

    /// <summary> Режим запуска. Значение по умолчанию — Debug. </summary>
    public static RunMode RunMode { get; set; } = RunMode.Debug;
    public static Saver Saver { get; set; } = new();
    public static double FixedTime { get; set; } = Constants.FixedTimeLimit;
    
    /// <summary> Стандартное имя сцены, которая изначально находится в игре. </summary>
    public const string StandardSceneName = "StandardScene";
    public const string StandardPlaceName = "StandardPlace";

    #endregion

    static Game()
    {
        Lists = new SortedList<GameAssetType, IDictionary>
        {
            { GameAssetType.Scene, PrivateScenes },
            { GameAssetType.Place, PrivatePlaces },
            { GameAssetType.GameObject, Objects },
            { GameAssetType.Script, PrivateScripts },
            { GameAssetType.Trigger, PrivateTriggers },
            { GameAssetType.Resource, PrivateResources },
            { GameAssetType.Model, PrivateModels }
        };
        var place = new Place();
        Add(StandardSceneName, new Scene(place));
        Add(StandardPlaceName, place, StandardSceneName);
    }

    /// <summary>
    /// Запуск окна с игрой.
    /// </summary>
    /// <param name="gameWindowSettings">Игровые настройки.</param>
    /// <param name="nativeWindowSettings">Настройки окна.</param>
    /// <exception cref="Exception">при повторном вызов метода.</exception>
    public static void Run(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
    {
        if (_isRunning)
            throw new Exception("Game is already running.");
        
        _isRunning = true;

        using var window = new TyzeWindow(gameWindowSettings, nativeWindowSettings);
        window.IsDebugMode = RunMode == RunMode.Debug;
        window.Scenes = PrivateScenes.Select(pair => pair.Value).ToList();
        window.Scripts = FrameScripts;
        window.CurrentSceneIndex = _currentSceneIndex;
        window.Run();
        _isRunning = false;

        if (FixedExecute.Status == TaskStatus.Running)
            FixedExecute.Wait(100);
    }

    #region SaveLoadMethods
    
    public static void Save(string fileName)
    {
        if (PrivateScenes.Count == 0 || PrivatePlaces.Count == 0 || Objects.Count == 0)
            return;

        var saveObject = new SaveObject(_currentSceneIndex, PrivatePlaces.Count, Objects.Count);

        foreach (var (_, place) in PrivatePlaces)
            saveObject.AddPlaceObjects(place);

        foreach (var (_, obj) in Objects.Where(localObj => localObj.Value.SaveStatus))
            saveObject.AddSaveObjectState(obj);

        var bytes = saveObject.Save();
        Saver.Save(bytes, fileName);
    }

    public static bool Load(string fileName)
    {
        IMaterial BytesToMaterial(byte[] data)
        {
            var floats = Vector.ToFloats(data, 4).ToArray();
            
            return new Material(floats[0], floats[1], floats[2], floats[3]);
        }

        var withErrors = false;
        var bytes = Saver.Load(fileName);
        var saveObject = SaveObject.GetByBytes(bytes);
        _currentSceneIndex = saveObject.CurrentSceneIndex;
        
        foreach (var (_, obj) in Objects)
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

            obj.SetModel(PrivateModels.FirstOrDefault(model => model.Value.Id == state.ModelId).Value);
            obj.Transform.Texture = PrivateResources.FirstOrDefault(resource => 
                resource.Value.Id == state.ResourceId).Value;
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
            
            obj.Transform.Position = Vector.ToVector3(state.Position);
            obj.Transform.Scale = Vector.ToVector3(state.Scale);
            obj.Transform.Rotation = Vector.ToVector3(state.Rotation);
            obj.Transform.Color = Vector.ToVector4(state.Color);
            obj.Transform.Visibility = (Visibility)state.VisibilityType;
            obj.Transform.Visual = (BodyVisualType)state.Visual;
            obj.Body.CollisionLayer = state.CollisionLayer;
            obj.Body.SetMaterial(BytesToMaterial(state.Material));
            obj.Body.Force = Vector.ToVector3(state.Force);
            obj.Body.GravityForce = Vector.ToVector3(state.GravityForce);
            obj.Body.IsEnabled = state.IsEnabled;
        }
        
        foreach (var (_, place) in PrivatePlaces)
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
                    obj = Objects.First(localObj => localObj.Value.Id == id).Value;
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
    
    /// <summary>
    /// Добавление объекта сцены в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="scene">Сцена.</param>
    public static void Add([NotNull] string name, [NotNull] IScene scene)
    {
        TryAddName(name, GameAssetType.Scene);
        TryAddUId(scene, GameAssetType.Scene);
        PrivateScenes.Add(name, scene);
    }

    public static void Add([NotNull] string name, [NotNull] IPlace place, string sceneOrPlaceName)
    {
        if (PrivatePlaces.ContainsKey(sceneOrPlaceName))
            PrivatePlaces[sceneOrPlaceName].NeighbourPlaces.Add(place);
        else if (PrivateScenes.ContainsKey(sceneOrPlaceName))
            PrivateScenes[sceneOrPlaceName].CurrentPlace = place;
        else
            throw new ArgumentException("Scene or place with this name doesn't exists.", nameof(sceneOrPlaceName));
        
        TryAddName(name, GameAssetType.Place);
        TryAddUId(place, GameAssetType.Place);
        PrivatePlaces.Add(name, place);
    }
    
    /// <summary>
    /// Добавление игрового объекта в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="obj">Игровой объект.</param>
    /// <param name="placeName"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void Add([NotNull] string name, [NotNull] IGameObject obj, [NotNull] string placeName)
    {
        if (!PrivatePlaces.ContainsKey(placeName))
            throw new ArgumentException("Place with this name doesn't exists.", nameof(placeName));
        
        TryAddName(name, GameAssetType.GameObject);
        TryAddUId(obj, GameAssetType.GameObject);
        PrivatePlaces[placeName].GameObjects.Add(obj);
        Objects.Add(name, obj);
        LoadQueue.Add(obj);
    }
    
    /// <summary>
    /// Добавление скрипта в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="script">Скрипт.</param>
    /// <param name="isFrameDependent">Истина, если скрипт должен выполняться в цикле, иначе скрипт будет выполняться вручную.</param>
    public static void Add([NotNull] string name, [NotNull] IScript script, bool isFrameDependent = true)
    {
        TryAddName(name, GameAssetType.Script);
        TryAddUId(script, GameAssetType.Script);
        
        if (isFrameDependent)
            FrameScripts.Add(script);
        
        PrivateScripts.Add(name, script);
    }

    /// <summary>
    /// Добавление триггера в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="trigger">Триггер.</param>
    public static void Add([NotNull] string name, [NotNull] ITrigger trigger)
    {
        TryAddName(name, GameAssetType.Trigger);
        TryAddUId(trigger, GameAssetType.Trigger);
        PrivateTriggers.Add(name, trigger);
    }

    /// <summary>
    /// Добавление ресурса в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="resource">Ресурс.</param>
    /// <exception cref="ArgumentException">Ресурс с таким же путём до файла уже существует.</exception>
    public static void Add([NotNull] string name, [NotNull] IResource resource)
    {
        if (PrivateResources.Select(pair => pair.Value.Path).Any(path => string.CompareOrdinal(path, resource.Path) == 0))
            throw new ArgumentException("Resource with this path already exists.", nameof(resource));
        
        TryAddName(name, GameAssetType.Resource);
        TryAddUId(resource, GameAssetType.Resource);
        PrivateResources.Add(name, resource);
    }

    /// <summary>
    /// Добавление модели в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="model">Модель.</param>
    public static void Add([NotNull] string name, [NotNull] IModel model)
    {
        TryAddName(name, GameAssetType.Model);
        TryAddUId(model, GameAssetType.Model);
        PrivateModels.Add(name, model);
    }

    #endregion

    #region RemoveMethods

    /// <summary>
    /// Удаление любого объекта из игры по имени.
    /// </summary>
    /// <param name="name">Уникальное имя объекта.</param>
    public static void Remove([NotNull] string name)
    {
        if (!Names.ContainsKey(name))
            return;
        
        var type = Names[name];
        Names.Remove(name);
        var obj = (IUIdObject)Lists[type][name];
        
        switch (obj)
        {
            case null:
                return;
            case IScript script:
                FrameScripts.Remove(script);
                break;
        }

        UIds.Remove(obj.Id);
        Lists[type].Remove(name);
    }

    /// <summary>
    /// Удаление любого объекта из игры по UID.
    /// </summary>
    /// <param name="id">UID объекта.</param>
    public static void Remove(UId id)
    {
        if (!UIds.ContainsKey(id))
            return;
        
        var type = UIds[id];
        UIds.Remove(id);
        var name = string.Empty;
        var obj = new object();

        foreach (DictionaryEntry entry in Lists[type])
        {
            var idObj = (IUIdObject)entry.Value;
            
            if (idObj is null)
                continue;
            
            if (idObj.Id != id)
                continue;

            name = (string)entry.Key;
            obj = entry.Value;
            break;
        }

        Names.Remove(name);
        
        if (obj is IScript script)
            FrameScripts.Remove(script);
        
        Lists[type].Remove(name);
    }

    #endregion

    internal static void StartFixedExecute()
    {
        Thread.Sleep(50);
        
        _fixedTime = Stopwatch.StartNew();
        FixedExecute.Start();
    }

    private static void TryAddUId(IUIdObject obj, GameAssetType type)
    {
        while (UIds.ContainsKey(obj.Id))
            obj.Id = new UId();

        UIds.Add(obj.Id, type);
    }
    
    private static void TryAddName(string name, GameAssetType type)
    {
        if (Names.ContainsKey(name))
            throw new ArgumentException("Object with this name already exists.", nameof(name));

        Names.Add(name, type);
    }
}