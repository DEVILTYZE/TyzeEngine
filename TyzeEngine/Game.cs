using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using TyzeEngine.GameStructure;
using TyzeEngine.Interfaces;
using TyzeEngine.Physics;

namespace TyzeEngine;

public static class Game
{
    #region PropertiesAndFields

    private static readonly Dictionary<UId, GameResourceType> UIds = new();
    private static readonly Dictionary<string, GameResourceType> Names = new();
    private static readonly SortedList<string, IScene> PrivateScenes = new();
    private static readonly SortedList<string, ISpace> PrivateSpaces = new();
    private static readonly SortedList<string, IGameObject> Objects = new();
    private static readonly SortedList<string, IScript> PrivateScripts = new();
    private static readonly SortedList<string, IResource> PrivateResources = new();
    private static readonly SortedList<string, IModel> PrivateModels = new();
    private static readonly SortedList<string, IMaterial> PrivateMaterials = new();
    private static bool _isRunning;
    private static readonly IReadOnlyDictionary<GameResourceType, IDictionary> Lists;
    private static Stopwatch _fixedTime;
    private static readonly Task FixedExecute = new(() =>
    {
        while (_isRunning)
        {
            FrameTimeState.FixedTime = (float)_fixedTime.Elapsed.TotalSeconds;
            
            if (FrameTimeState.FixedTime <= Settings.FixedTime) 
                continue;
            
            PhysicsWorld.Step(CurrentScene.GetCurrentGameObjects());
            // For используется для безопасного удаления скриптов.
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < FrameScripts.Count; i++)
                FrameScripts[i].InternalFixedExecute();
            
            _fixedTime.Restart();
        }
    });
    
    internal static IReadOnlyDictionary<string, IScene> Scenes => PrivateScenes;
    internal static IReadOnlyDictionary<string, ISpace> Spaces => PrivateSpaces;
    internal static IReadOnlyDictionary<string, IGameObject> GameObjects => Objects;
    internal static IReadOnlyDictionary<string, IScript> Scripts => PrivateScripts;
    internal static IReadOnlyDictionary<string, IResource> Resources => PrivateResources;
    internal static IReadOnlyDictionary<string, IModel> Models => PrivateModels;
    internal static IReadOnlyDictionary<string, IMaterial> Materials => PrivateMaterials;
    internal static readonly List<IScript> FrameScripts = new();
    internal static readonly SortedList<BodyVisualType, Shader> Shaders = new();
    internal static IScene CurrentScene { get; set; }
    
    public static GameSettings Settings { get; }
    public static IPhysicsWorld PhysicsWorld { get; set; } = new DynamicWorld();
    
    /// <summary> Стандартное имя сцены, которая изначально находится в игре. </summary>
    public const string StandardSceneName = "StandardScene";
    /// <summary> Стандартное имя пространства, которое изначально находится в игре. </summary>
    public const string StandardSpaceName = "StandardSpace";
    public const string StandardWorldLightName = "StandardWorldLight";

    #endregion

    static Game()
    {
        Lists = new SortedList<GameResourceType, IDictionary>
        {
            { GameResourceType.Scene, PrivateScenes },
            { GameResourceType.Space, PrivateSpaces },
            { GameResourceType.GameObject, Objects },
            { GameResourceType.Script, PrivateScripts },
            { GameResourceType.Resource, PrivateResources },
            { GameResourceType.Model, PrivateModels },
            { GameResourceType.Material, PrivateMaterials }
        };
        Add(StandardSceneName, StandardSpaceName, new Scene());
        Add(StandardWorldLightName, StandardSpaceName, new LightObject(LightType.Direction));
        CurrentScene = Scene.Find(StandardSceneName);

        var settings = new NativeWindowSettings
        {
            WindowBorder = WindowBorder.Fixed,
            Size = new Vector2i(1000, 750),
            Title = "Test application",
            Flags = ContextFlags.ForwardCompatible,
            APIVersion = new Version(3, 3)
        };
        Settings = new GameSettings(settings);
    }

    /// <summary>
    /// Запуск окна с игрой.
    /// </summary>
    /// <exception cref="Exception">Повторный вызов метода.</exception>
    public static void Run()
    {
        if (_isRunning)
            throw new Exception("Game is already running.");
        
        using (Settings.Window)
        {
            _isRunning = true;
            Settings.Window.Run();
            _isRunning = false;
            
            if (FixedExecute.Status == TaskStatus.Running)
                FixedExecute.Wait(25);
            
            Unload();
        }
    }

    #region SaveLoadMethods
    
    public static void Save(string fileName)
    {
        if (PrivateScenes.Count == 0 || PrivateSpaces.Count == 0 || Objects.Count == 0)
            return;

        var saveObject = new SaveObject(PrivateSpaces.Count, Objects.Count);

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
        
        foreach (var (_, obj) in Objects)
        {
            if (!obj.SaveStatus)
                continue;
            
            var state = saveObject.ObjectStates.FirstOrDefault(localState => localState.Id == obj.Id);

            if (state.Equals(default))
            {
                withErrors = true;
                continue;
            }

            obj.Model = PrivateModels.FirstOrDefault(model => model.Value.Id == state.ModelId).Value;
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
            obj.Color = Color4.FromXyz(Vector.ToVector4(state.Color));
            obj.Visibility = (Visibility)state.VisibilityType;
            ((GameObject)obj).VisualType = (BodyVisualType)state.Visual;
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

                var obj = Objects.FirstOrDefault(localObj => localObj.Value.Id == id).Value;

                if (obj is null)
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
        
        Add(name, scene, GameResourceType.Scene);
        Add(spaceName, scene.CurrentSpace, GameResourceType.Space);
        scene.CurrentSpace.SceneOrSpaceName = name;
    }

    /// <summary>
    /// Добавление объекта пространства в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="space">Пространство.</param>
    /// <param name="spaceName">Имя пространства, к которому должно быть привязано новое пространство.</param>
    /// <exception cref="ArgumentNullException">Имя привязанного пространства равно null.</exception>
    /// <exception cref="ArgumentException">
    /// Не существует пространства, к которому будет привязано новое пространство.
    /// </exception>
    public static void Add([NotNull] string name, [NotNull] string spaceName, [NotNull] ISpace space)
    {
        if (string.IsNullOrEmpty(spaceName))
            throw new ArgumentNullException(nameof(spaceName), "Linked space name was null.");
        
        if (PrivateSpaces.ContainsKey(spaceName))
            PrivateSpaces[spaceName].NeighbourSpaces.Add(space);
        else
            throw new ArgumentException("Space with this name doesn't exists.", nameof(spaceName));
        
        Add(name, space, GameResourceType.Space);
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
        
        Add(name, obj, GameResourceType.GameObject);
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
        Add(name, script, GameResourceType.Script);
        
        if (isFrameDependent)
            FrameScripts.Add(script);
    }

    /// <summary>
    /// Добавление объекта ресурса в игру. Уже добавленные ресурсы можно добавить снова.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="resource">Ресурс.</param>
    public static void Add([NotNull] string name, [NotNull] IResource resource) =>
        Add(name, resource, GameResourceType.Resource);

    /// <summary>
    /// Добавление объекта модели в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="model">Модель.</param>
    public static void Add([NotNull] string name, [NotNull] IModel model)
    {
        Add(name, model, GameResourceType.Model);
        LoadQueue.Add(model);
    }

    /// <summary>
    /// Добавление объекта материала в игру.
    /// </summary>
    /// <param name="name">Уникальное имя.</param>
    /// <param name="material">Материал.</param>
    public static void Add([NotNull] string name, [NotNull] IMaterial material) => 
        Add(name, material, GameResourceType.Material);

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
        if (!_isRunning)
            throw new Exception("Game is not running.");
        
        Thread.Sleep(25);
        
        _fixedTime = Stopwatch.StartNew();
        FixedExecute.Start();
    }

    internal static void SetShaders()
    {
        var paths = new (string VertShader, string FragShader)[]
        {
            (Constants.ShaderVertObjectPath, Constants.ShaderFragObjectPath),
            (Constants.ShaderVertLinePath, Constants.ShaderFragLinePath)
        };
        var types = new[] { BodyVisualType.Object, BodyVisualType.Line };

        for (var i = 0; i < types.Length; ++i)
        {
            var shader = new Shader(paths[i].VertShader, paths[i].FragShader);
            Shaders.Add(types[i], shader);
        }
    }

    private static void Add(string name, IGameResource obj, GameResourceType type)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name), "Game resource name was null.");
        if (obj is null)
            throw new ArgumentNullException(nameof(obj), "Game resource was null.");
        
        TryAddName(name, type);
        TryAddUId(obj, type);
        Lists[type].Add(name, obj);
    }
    
    private static void TryAddName(string name, GameResourceType type)
    {
        if (Names.ContainsKey(name))
            throw new ArgumentException("Object with this name already exists.", nameof(name));

        Names.Add(name, type);
    }

    private static void TryAddUId(IGameResource obj, GameResourceType type)
    {
        while (UIds.ContainsKey(obj.Id))
            obj.Id = new UId();

        UIds.Add(obj.Id, type);
    }

    private static void Unload()
    {
        foreach (var (_, scene) in Scenes)
            scene.Dispose();
        
        foreach(var (_, shader) in Shaders)
            shader.Dispose();
    }
}