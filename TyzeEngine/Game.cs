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
    private static bool _isRunning;
    private static int _currentSceneIndex;
    private static readonly IReadOnlyDictionary<GameAssetType, IDictionary> Lists;
    private static Stopwatch _fixedTime;
    private static readonly Task FixedExecute = new(() =>
    {
        while (_isRunning)
        {
            if (_fixedTime.Elapsed.TotalSeconds <= Settings.FixedTime) 
                continue;
            
            FrameScripts.ForEach(script => script.FixedExecute());
            _fixedTime.Restart();
        }
    });

    public static GameSettings Settings { get; } = new();
    
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
    
    /// <summary> Стандартное имя сцены, которая изначально находится в игре. </summary>
    public const string StandardSceneName = "StandardScene";
    /// <summary> Стандартное имя пространства, которое изначально находится в игре. </summary>
    public const string StandardSpaceName = "StandardSpace";
    public const string StandardWorldLightName = "StandardWorldLight";

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
        Add(StandardSceneName, StandardSpaceName, new Scene());
        var light = new DirectionLight();
        Add(StandardWorldLightName, StandardSpaceName, light);
    }

    /// <summary>
    /// Запуск окна с игрой.
    /// </summary>
    /// <param name="nativeWindowSettings">Настройки окна.</param>
    /// <exception cref="Exception">Повторный вызов метода.</exception>
    public static void Run(NativeWindowSettings nativeWindowSettings)
    {
        if (_isRunning)
            throw new Exception("Game is already running.");
        
        _isRunning = true;
        var gameWindowSettings = new GameWindowSettings { RenderFrequency = 144, UpdateFrequency = 144 };
        
        using (Settings.Window = new TyzeWindow(gameWindowSettings, nativeWindowSettings))
        {
            Settings.Window.IsDebugMode = Settings.RunMode == RunMode.Debug;
            Settings.Window.Scenes = PrivateScenes.Select(pair => pair.Value).ToList();
            Settings.Window.Scripts = FrameScripts;
            Settings.Window.CurrentSceneIndex = _currentSceneIndex;
            Settings.Window.Run();
            _isRunning = false;
        }

        Settings.Window = null;
        
        if (FixedExecute.Status == TaskStatus.Running)
            FixedExecute.Wait(100);
    }

    #region SaveLoadMethods
    
    public static void Save(string fileName)
    {
        if (PrivateScenes.Count == 0 || PrivateSpaces.Count == 0 || Objects.Count == 0)
            return;

        var saveObject = new SaveObject(_currentSceneIndex, PrivateSpaces.Count, Objects.Count);

        foreach (var (_, space) in PrivateSpaces)
            saveObject.AddSpaceObjects(space);

        foreach (var (_, obj) in Objects.Where(localObj => localObj.Value.SaveStatus))
            saveObject.AddSaveObjectState(obj);

        var bytes = saveObject.Save();
        Settings.Saver.Save(bytes, fileName);
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
            ((Visual)obj.Visual).Type = (BodyVisualType)state.Visual;
            obj.Body.CollisionLayer = state.CollisionLayer;
            //obj.Body.SetMaterial(BytesToMaterial(state.Material));
            obj.Body.Force = Vector.ToVector3(state.Force);
            obj.Body.GravityForce = Vector.ToVector3(state.GravityForce);
            obj.Body.IsEnabled = state.IsEnabled;
        }
        
        foreach (var (_, space) in PrivateSpaces)
        {
            var index = -1;
            
            for (var i = 0; i < saveObject.SpaceIds.Length; ++i)
            {
                if (saveObject.SpaceIds[i] != space.Id) 
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
                if (space.GameObjects.Any(obj => obj.Id == id))
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

                space.GameObjects.Add(obj);
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
    /// <param name="spaceName">Уникальное имя первого пространства в сцене.</param>
    /// <param name="scene">Сцена.</param>
    /// <exception cref="ArgumentNullException">Имя пространства равно null.</exception>
    public static void Add([NotNull] string name, [NotNull] string spaceName, [NotNull] IScene scene)
    {
        if (string.IsNullOrEmpty(spaceName))
            throw new ArgumentNullException(nameof(spaceName), "Space name was null.");
        
        Add(name, scene, GameAssetType.Scene);
        Add(spaceName, scene.CurrentSpace, GameAssetType.Space);
        scene.CurrentSpace.SceneOrSpaceName = name;
    }

    /// <summary>
    /// Добавление объекта пространства в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="space">Пространство.</param>
    /// <param name="spaceName">Имя сцены или пространства, к которому должно быть привязано новое пространство.</param>
    /// <exception cref="ArgumentNullException">Имя привязанного пространства равно null.</exception>
    /// <exception cref="ArgumentException">
    /// Не существует сцены или пространства, к которому будет привязано новое пространство.
    /// </exception>
    public static void Add([NotNull] string name, [NotNull] string spaceName, [NotNull] ISpace space)
    {
        if (string.IsNullOrEmpty(spaceName))
            throw new ArgumentNullException(nameof(spaceName), "Linked space name was null.");
        
        if (PrivateSpaces.ContainsKey(spaceName))
            PrivateSpaces[spaceName].NeighbourSpaces.Add(space);
        else
            throw new ArgumentException("Space with this name doesn't exists.", nameof(spaceName));
        
        Add(name, space, GameAssetType.Space);
        space.SceneOrSpaceName = spaceName;
    }
    
    /// <summary>
    /// Добавление игрового объекта в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="obj">Игровой объект.</param>
    /// <param name="spaceName">Имя пространства, к которому будет привязан новый объект.</param>
    /// <exception cref="ArgumentNullException">Имя пространства равно null.</exception>
    /// <exception cref="ArgumentException">Не существует пространства, к которому будет привязан новый объект.</exception>
    public static void Add([NotNull] string name, [NotNull] string spaceName, [NotNull] IGameObject obj)
    {
        if (string.IsNullOrEmpty(spaceName))
            throw new ArgumentNullException(nameof(spaceName), "Space name was null.");
        if (!PrivateSpaces.ContainsKey(spaceName))
            throw new ArgumentException("Space with this name doesn't exists.", nameof(spaceName));
        
        Add(name, obj, GameAssetType.GameObject);
        PrivateSpaces[spaceName].GameObjects.Add(obj);
        obj.SpaceName = spaceName;
    }
    
    /// <summary>
    /// Добавление объекта скрипта в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="script">Скрипт.</param>
    /// <param name="isFrameDependent">
    /// Истина, если скрипт должен выполняться в цикле, иначе скрипт будет выполняться при вызове функции Execute или
    /// FixedExecute.
    /// </param>
    public static void Add([NotNull] string name, [NotNull] IScript script, bool isFrameDependent = true)
    {
        Add(name, script, GameAssetType.Script);
        
        if (isFrameDependent)
            FrameScripts.Add(script);
    }

    /// <summary>
    /// Добавление объекта триггера в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="trigger">Триггер.</param>
    public static void Add([NotNull] string name, [NotNull] ITrigger trigger) => Add(name, trigger, GameAssetType.Trigger);

    /// <summary>
    /// Добавление объекта ресурса в игру. Уже добавленные ресурсы можно добавить снова.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="resource">Ресурс.</param>
    public static void Add([NotNull] string name, [NotNull] IResource resource) => 
        Add(name, resource, GameAssetType.Resource);

    /// <summary>
    /// Добавление объекта модели в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="model">Модель.</param>
    public static void Add([NotNull] string name, [NotNull] IModel model)
    {
        Add(name, model, GameAssetType.Model);
        LoadQueue.Add(model);
    }

    /// <summary>
    /// Добавление объекта материала в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="material">Материал.</param>
    public static void Add([NotNull] string name, [NotNull] IMaterial material) => 
        Add(name, material, GameAssetType.Material);

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
        string name = null;

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
            (Constants.ShaderVertObjectPath, Constants.ShaderFragObjectPath),
            (Constants.ShaderVertLinePath, Constants.ShaderFragLinePath)
        };
        var types = new[] { BodyVisualType.Object, BodyVisualType.Line };

        for (var i = 0; i < types.Length; ++i)
        {
            var shader = new Shader(paths[i].Item1, paths[i].Item2);
            Shaders.Add(types[i], shader);
        }
    }

    private static void Add(string name, IGameResource obj, GameAssetType type)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name), "Game resource name was null.");
        if (obj is null)
            throw new ArgumentNullException(nameof(obj), "Game resource was null.");
        
        TryAddName(name, type);
        TryAddUId(obj, type);
        Lists[type].Add(name, obj);
    }
    
    private static void TryAddName(string name, GameAssetType type)
    {
        if (Names.ContainsKey(name))
            throw new ArgumentException("Object with this name already exists.", nameof(name));

        Names.Add(name, type);
    }

    private static void TryAddUId(IGameResource obj, GameAssetType type)
    {
        while (UIds.ContainsKey(obj.Id))
            obj.Id = new UId();

        UIds.Add(obj.Id, type);
    }
}