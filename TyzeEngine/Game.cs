using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;
using TyzeEngine.Materials;
using TyzeEngine.Objects;
using TyzeEngine.Resources;

namespace TyzeEngine;

public static class Game
{
    #region PropertiesAndFields

    private static readonly Dictionary<UId, GameAssetType> UIds = new();
    private static readonly Dictionary<string, GameAssetType> Names = new();
    private static readonly SortedList<string, IScene> PrivateScenes = new();
    private static readonly SortedList<string, ISpace> PrivateSpaces = new();
    private static readonly SortedList<string, IGameObject> Objects = new();
    private static readonly SortedList<string, IScript> PrivateScripts = new();
    private static readonly SortedList<string, ITrigger> PrivateTriggers = new();
    private static readonly SortedList<string, IResource> PrivateResources = new();
    private static readonly SortedList<string, IModel> PrivateModels = new();
    private static readonly SortedList<string, IMaterial> PrivateMaterials = new();
    private static int _currentSceneIndex;
    private static readonly IReadOnlyDictionary<GameAssetType, IDictionary> Lists;
    private static Stopwatch _fixedTime;
    private static readonly Task FixedExecute = new(() =>
    {
        while (IsRunning)
        {
            if (_fixedTime.Elapsed.TotalSeconds <= FixedTime) 
                continue;
            
            foreach (var script in FrameScripts)
                script.FixedExecute();
            
            _fixedTime.Restart();
        }
    });

    internal static IReadOnlyDictionary<string, IScene> Scenes => PrivateScenes;
    internal static IReadOnlyDictionary<string, ISpace> Spaces => PrivateSpaces;
    internal static IReadOnlyDictionary<string, IGameObject> GameObjects => Objects;
    internal static IReadOnlyDictionary<string, IScript> Scripts => PrivateScripts;
    internal static IReadOnlyDictionary<string, ITrigger> Triggers => PrivateTriggers;
    internal static IReadOnlyDictionary<string, IResource> Resources => PrivateResources;
    internal static IReadOnlyDictionary<string, IModel> Models => PrivateModels;
    internal static IReadOnlyDictionary<string, IMaterial> Materials => PrivateMaterials;
    internal static readonly List<IScript> FrameScripts = new();
    internal static readonly SortedList<BodyVisualType, Shader> Shaders = new();

    /// <summary> Режим запуска. Значение по умолчанию — Debug. </summary>
    public static RunMode RunMode { get; set; } = RunMode.Debug;
    public static Saver Saver { get; set; } = new();
    public static double FixedTime { get; set; } = Constants.FixedTimeLimit;
    public static bool IsRunning { get; private set; }
    
    /// <summary> Стандартное имя сцены, которая изначально находится в игре. </summary>
    public const string StandardSceneName = "StandardScene";
    public const string StandardSpaceName = "StandardPlace";

    #endregion

    static Game()
    {
        Lists = new SortedList<GameAssetType, IDictionary>
        {
            { GameAssetType.Scene, PrivateScenes },
            { GameAssetType.Space, PrivateSpaces },
            { GameAssetType.GameObject, Objects },
            { GameAssetType.Script, PrivateScripts },
            { GameAssetType.Trigger, PrivateTriggers },
            { GameAssetType.Resource, PrivateResources },
            { GameAssetType.Model, PrivateModels },
            { GameAssetType.Material, PrivateMaterials }
        };
        var space = new Space();
        Add(StandardSceneName, new Scene(space));
        Add(StandardSpaceName, space, StandardSceneName);
    }

    /// <summary>
    /// Запуск окна с игрой.
    /// </summary>
    /// <param name="nativeWindowSettings">Настройки окна.</param>
    /// <exception cref="Exception">при повторном вызов метода.</exception>
    public static void Run(NativeWindowSettings nativeWindowSettings)
    {
        if (IsRunning)
            throw new Exception("Game is already running.");
        
        IsRunning = true;
        var gameWindowSettings = new GameWindowSettings { RenderFrequency = 144, UpdateFrequency = 144 };
        using var window = new TyzeWindow(gameWindowSettings, nativeWindowSettings);
        window.IsDebugMode = RunMode == RunMode.Debug;
        window.Scenes = PrivateScenes.Select(pair => pair.Value).ToList();
        window.Scripts = FrameScripts;
        window.CurrentSceneIndex = _currentSceneIndex;
        window.Run();
        IsRunning = false;

        if (FixedExecute.Status == TaskStatus.Running)
            FixedExecute.Wait(100);
    }

    #region SaveLoadMethods
    
    public static void Save(string fileName)
    {
        if (PrivateScenes.Count == 0 || PrivateSpaces.Count == 0 || Objects.Count == 0)
            return;

        var saveObject = new SaveObject(_currentSceneIndex, PrivateSpaces.Count, Objects.Count);

        foreach (var (_, place) in PrivateSpaces)
            saveObject.AddPlaceObjects(place);

        foreach (var (_, obj) in Objects.Where(localObj => localObj.Value.SaveStatus))
            saveObject.AddSaveObjectState(obj);

        var bytes = saveObject.Save();
        Saver.Save(bytes, fileName);
    }

    public static bool Load(string fileName)
    {
        var withErrors = false;
        var bytes = Saver.Load(fileName);
        var saveObject = SaveObject.GetByBytes(bytes);
        _currentSceneIndex = saveObject.CurrentSceneIndex;
        
        foreach (var (_, obj) in Objects)
        {
            if (!obj.SaveStatus)
                continue;
            
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

            obj.Model = PrivateModels.FirstOrDefault(model => model.Value.Id == state.ModelId).Value;
            obj.Visual.Texture = PrivateResources.FirstOrDefault(resource => 
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
            obj.Visual.Color = Color4.FromXyz(Vector.ToVector4(state.Color));
            obj.Visual.Visibility = (Visibility)state.VisibilityType;
            obj.Visual.Type = (BodyVisualType)state.Visual;
            obj.Body.CollisionLayer = state.CollisionLayer;
            //obj.Body.SetMaterial(BytesToMaterial(state.Material));
            obj.Body.Force = Vector.ToVector3(state.Force);
            obj.Body.GravityForce = Vector.ToVector3(state.GravityForce);
            obj.Body.IsEnabled = state.IsEnabled;
        }
        
        foreach (var (_, place) in PrivateSpaces)
        {
            var index = -1;
            
            for (var i = 0; i < saveObject.SpaceIds.Length; ++i)
            {
                if (saveObject.SpaceIds[i] != place.Id) 
                    continue;
                
                index = i;
                break;
            }
            
            if (index == -1)
            {
                withErrors = true;
                continue;
            }

            foreach (var id in saveObject.SpaceObjects[index])
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

    /// <summary>
    /// Добавление объекта пространства в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="space">Пространство.</param>
    /// <param name="sceneOrSpaceName">Имя сцены или пространства, к которому должно быть привязано новое пространство.</param>
    /// <exception cref="ArgumentException">Не существует сцены или пространства, к которому будет привязано новое пространство.</exception>
    public static void Add([NotNull] string name, [NotNull] ISpace space, string sceneOrSpaceName)
    {
        if (PrivateSpaces.ContainsKey(sceneOrSpaceName))
            PrivateSpaces[sceneOrSpaceName].NeighbourSpaces.Add(space);
        else if (PrivateScenes.ContainsKey(sceneOrSpaceName))
            PrivateScenes[sceneOrSpaceName].CurrentPlace = space;
        else
            throw new ArgumentException("Scene or space with this name doesn't exists.", nameof(sceneOrSpaceName));
        
        TryAddName(name, GameAssetType.Space);
        TryAddUId(space, GameAssetType.Space);
        PrivateSpaces.Add(name, space);
        space.SceneOrSpaceName = sceneOrSpaceName;
    }
    
    /// <summary>
    /// Добавление игрового объекта в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="obj">Игровой объект.</param>
    /// <param name="spaceName">Имя пространства, к которому будет привязан новый объект.</param>
    /// <exception cref="ArgumentException">Не существует пространства, к которому будет привязан новый объект.</exception>
    public static void Add([NotNull] string name, [NotNull] IGameObject obj, [NotNull] string spaceName)
    {
        if (!PrivateSpaces.ContainsKey(spaceName))
            throw new ArgumentException("Space with this name doesn't exists.", nameof(spaceName));
        
        TryAddName(name, GameAssetType.GameObject);
        TryAddUId(obj, GameAssetType.GameObject);
        PrivateSpaces[spaceName].GameObjects.Add(obj);
        obj.SpaceName = spaceName;
        Objects.Add(name, obj);
        LoadQueue.Add(obj);
    }
    
    /// <summary>
    /// Добавление объекта скрипта в игру.
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
    /// Добавление объекта триггера в игру.
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
    /// Добавление объекта ресурса в игру.
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
    /// Добавление объекта модели в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="model">Модель.</param>
    public static void Add([NotNull] string name, [NotNull] IModel model)
    {
        TryAddName(name, GameAssetType.Model);
        TryAddUId(model, GameAssetType.Model);
        PrivateModels.Add(name, model);
    }

    /// <summary>
    /// Добавление объекта материала в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="material">Материал.</param>
    public static void Add([NotNull] string name, [NotNull] IMaterial material)
    {
        TryAddName(name, GameAssetType.Model);
        TryAddUId(material, GameAssetType.Model);
        PrivateMaterials.Add(name, material);
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
        var obj = (IGameResource)Lists[type][name];

        if (obj is null)
            throw new NullReferenceException("Object was null.");
        
        obj.Remove();
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
        IGameResource gameResource = null;
        var name = string.Empty;

        foreach (DictionaryEntry entry in Lists[type])
        {
            gameResource = (IGameResource)entry.Value;

            if (gameResource is null || gameResource.Id != id) 
                continue;
            
            name = (string)entry.Key;
            break;
        }

        if (gameResource is null || string.IsNullOrEmpty(name))
            throw new NullReferenceException("Object was null or name was empty.");
        
        gameResource.Remove();
        Names.Remove(name);
        Lists[type].Remove(name);
    }

    #endregion

    internal static void StartFixedExecute()
    {
        Thread.Sleep(50);
        
        _fixedTime = Stopwatch.StartNew();
        FixedExecute.Start();
    }

    internal static void SetShaders()
    {
        var paths = new[]
        {
            Constants.ShaderVertColorPath, Constants.ShaderFragColorPath,
            Constants.ShaderVertTexturePath, Constants.ShaderFragTexturePath,
            Constants.ShaderVertLightPath, Constants.ShaderFragLightPath
        };
        var types = new[]
        {
            BodyVisualType.Color, BodyVisualType.Texture, BodyVisualType.Light
        };

        for (var i = 0; i < types.Length; ++i)
        {
            var shader = new Shader(paths[i * 2], paths[i * 2 + 1]);
            Shaders.Add(types[i], shader);
        }
    }

    private static void TryAddUId(IGameResource obj, GameAssetType type)
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